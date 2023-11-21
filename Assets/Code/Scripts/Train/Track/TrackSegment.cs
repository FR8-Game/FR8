﻿using System;
using System.Collections;
using System.Collections.Generic;
using FR8.Runtime.CodeUtility;
using FR8.Runtime.Train.Splines;
using UnityEditor;
using UnityEngine;
using ColorUtility = HBCore.Utility.ColorUtility;

namespace FR8.Runtime.Train.Track
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrackSegment : MonoBehaviour, IEnumerable<Transform>
    {
        public const int TrackLayer = 12;

        // --- Constants ---
        public const float ConnectionDistance = 3.0f;
        public static readonly Spline.SplineProfile SplineProfile = Spline.Cubic;

        // --- Properties ---
        public TrackJunction junctionPrefab;

        [Space]
        [SerializeField] private int resolution = 100;

        [SerializeField] private bool closedLoop;
        [SerializeField] private bool conformToTerrain = true;

        // --- Internal Fields ---
        [SerializeField] private Connection startConnection = new();
        [SerializeField] private Connection endConnection = new();

        private Transform knotContainer;

        private float totalLength;
        private List<Vector3> points;
        private List<Vector3> velocities;
        private List<UnityEngine.Terrain> terrainList;

        // --- Properties ---
        public Connection StartConnection => startConnection;
        public Connection EndConnection => endConnection;
        public int Resolution => resolution;
        public int Count => KnotContainer().childCount + (closedLoop ? 1 : 0);

        public (List<Vector3>, List<Vector3>) GetBakeData()
        {
            return (new List<Vector3>(points), new List<Vector3>(velocities));
        }

        private void Awake()
        {
            BakeData();
        }

        private void Start()
        {
            LookForConnections();

            if (startConnection.other)
            {
                junctionPrefab.SpawnFromPrefab(this, startConnection.other, TrackJunction.ConnectionEnd.Start);
            }

            if (endConnection.other)
            {
                junctionPrefab.SpawnFromPrefab(this, endConnection.other, TrackJunction.ConnectionEnd.End);
            }
            
            var tree = transform.GetComponentsInChildren<Transform>();
            gameObject.layer = TrackLayer;
            foreach (var child in tree)
            {
                child.gameObject.layer = TrackLayer;
            }
        }

        private void OnDrawGizmosSelected()
        {
            var col = new Color(0f, 0.84f, 1f);
            DrawGizmos(true, col, ColorUtility.Invert(col));

            var others = FindObjectsOfType<TrackSegment>();
            foreach (var e in others)
            {
                e.DrawGizmos(false, col, ColorUtility.Invert(col));
            }
        }

        public void DrawGizmos(bool main, Color selectedColor, Color otherColor)
        {
            var linePoints = new List<Vector3>();
            linePoints.AddRange(points);

            foreach (var e in points)
            {
                #if UNITY_EDITOR
                Handles.color = new Color(1.0f, 0.6f, 0.0f, 1.0f);
                Handles.matrix = Matrix4x4.identity;
                Handles.DrawWireArc(e, Vector3.up, Vector3.forward, 360.0f, 1.0f);
                #endif
            }
            
            if (closedLoop) linePoints.Add(points[0]);

            GizmosDrawLine(main ? selectedColor : otherColor, 1.0f, linePoints.ToArray());
            DrawLineBetweenKnots();

            foreach (Transform e in KnotContainer())
            {
                DrawDistanceFromGround(e.position);
            }
        }

        private void DrawLineBetweenKnots()
        {
            var points = new List<Vector3>();
            foreach (Transform t in this)
            {
                points.Add(t.position);
            }

            GizmosDrawLine(new Color(1.0f, 1.0f, 1.0f, 0.4f), 0.3f, points.ToArray());
        }

        private void DrawDistanceFromGround(Vector3 p0)
        {
#if UNITY_EDITOR
            if (Physics.Raycast(p0, Vector3.down, out var hit))
            {
                var radius = Mathf.Min(2.0f, hit.distance * 2.0f);
                GizmosDrawLine(new Color(0.4f, 1.0f, 0.2f, 1.0f), 1.0f, p0, hit.point);
                Handles.DrawWireArc(hit.point, Vector3.up, Vector3.right, 360.0f, radius);
            }

            if (Physics.Raycast(p0, Vector3.up, out hit))
            {
                var radius = Mathf.Min(2.0f, hit.distance * 20.0f);
                GizmosDrawLine(new Color(0.4f, 1.0f, 0.2f, 1.0f), 1.0f, p0, hit.point);
                Handles.DrawWireArc(hit.point, Vector3.up, Vector3.right, 360.0f, radius);
            }
#endif
        }

        private void GizmosDrawLine(Color color, float width = 1.0f, params Vector3[] points)
        {
#if UNITY_EDITOR
            width *= 4.0f;

            Handles.color = color;
            Handles.DrawAAPolyLine(width, points);
#endif
        }

        public void BakeData()
        {
            // Bakes down the track segment to a list of line segments and their corresponding velocities.
            points = new List<Vector3>();
            velocities = new List<Vector3>();

            for (var i = 0; i < resolution; i++)
            {
                var p = i / (resolution - 1.0f);
                points.Add(SampleSpline(p, (spline, t) => spline.EvaluatePoint(t), i => (this[i].position, this[i].forward * this[i].localScale.z), Count));
                velocities.Add(SampleSpline(p, (spline, t) => spline.EvaluateVelocity(t), i => (this[i].position, this[i].forward * this[i].localScale.z), Count));
            }

            terrainList = new List<UnityEngine.Terrain>(FindObjectsOfType<UnityEngine.Terrain>());
        }

        public Vector3 KnotVelocity(int index) => SampleVelocity(GetKnotPercent(index));

        public int FromEnd(int i) => Count - i;

        public Vector3 SamplePoint(float t) => Sample(t, points, Vector3.LerpUnclamped);
        public Vector3 SampleVelocity(float t) => Sample(t, velocities, Vector3.Lerp);
        public Vector3 SampleTangent(float t) => SampleVelocity(t).normalized;

        public Vector3 Sample(float t0, IList<Vector3> bakeData, Func<Vector3, Vector3, float, Vector3> lerp)
        {
            var t1 = t0 * bakeData.Count;
            var i0 = Mathf.FloorToInt(t1);
            var i1 = i0 + 1;

            if (closedLoop)
            {
                i0 = (i0 % bakeData.Count + bakeData.Count) % bakeData.Count;
                i1 = (i1 % bakeData.Count + bakeData.Count) % bakeData.Count;
            }
            else
            {
                if (i1 >= bakeData.Count)
                {
                    i0 = bakeData.Count - 2;
                    i1 = bakeData.Count - 1;
                }

                if (i0 < 0)
                {
                    i0 = 0;
                    i1 = 1;
                }
            }

            var a = bakeData[i0];
            var b = bakeData[i1];

            return lerp(a, b, (t1 - i0) / (i1 - i0));
        }

        private static T SampleSpline<T>(float t, Func<Spline, float, T> callback, Func<int, (Vector3, Vector3)> list, int count)
        {
            t = Mathf.Clamp01(t);

            if (count < 2) return default;

            t *= count - 1;
            var i0 = Mathf.FloorToInt(t);
            if (i0 >= count - 2) i0 = count - 2;

            var (knotPos0, knotFwd0) = list(i0);
            var (knotPos1, knotFwd1) = list(i0 + 1);

            var l = (knotPos1 - knotPos0).magnitude;

            var p1 = knotPos0 + knotFwd0 * l / 3.0f;
            var p2 = knotPos1 - knotFwd1 * l / 3.0f;

            return callback(SplineProfile(knotPos0, p1, p2, knotPos1), t - i0);
        }

        public Vector3 TryConformToTerrain(Vector3 point)
        {
            return conformToTerrain ? TerrainUtility.GetPointOnTerrain(terrainList, point) : point;
        }

        public void OnValidate()
        {
            if (!Valid(this)) return;

            BakeData();

            var childCount = KnotContainer().childCount;
            resolution = Mathf.Max(resolution, childCount);
        }

        public float GetClosestPoint(Vector3 point, bool clamp)
        {
            if (points == null || points.Count == 0) BakeData();
            if (points.Count == 0) return default;

            FindClosestPair(point, out var best, out var other);
            var t = InterpolatePoints(point, best, other);

            if (clamp && !closedLoop) t = Mathf.Clamp01(t);

            return t;
        }

        private float InterpolatePoints(Vector3 point, int best, int other)
        {
            var a = points[best];
            var b = points[other];

            var v1 = b - a;
            var v2 = point - a;

            var dot = Vector3.Dot(v1.normalized, v2) / v1.magnitude;

            var closest = Mathf.LerpUnclamped(best / (points.Count - 1.0f), other / (points.Count - 1.0f), dot);
            return closest;
        }

        private void FindClosestPair(Vector3 point, out int best, out int other)
        {
            best = 0;
            var bestScore = float.MaxValue;
            for (var i = 0; i < points.Count; i++)
            {
                var score = (points[i] - point).sqrMagnitude;
                if (score > bestScore) continue;

                best = i;
                bestScore = score;
            }

            other = best + 1;
            if (other >= points.Count)
            {
                other--;
                best--;
            }
        }

        public float GetKnotPercent(int index)
        {
            return index / (float)Count;
        }

        public int GetKnotIndex(float t)
        {
            t = Mathf.Clamp(t, 0.0f, 0.99999f);

            var i = Mathf.FloorToInt(t * FromEnd(2)) + 1;
            return i;
        }

        public void LookForConnections()
        {
            if (!Valid(this)) return;

            var segments = FindObjectsOfType<TrackSegment>();

            // Update start and end
            CheckConnectionEnd(segments, startConnection, 0);
            CheckConnectionEnd(segments, endConnection, 1);
        }

        private void CheckConnectionEnd(IEnumerable<TrackSegment> segments, Connection connection, int end)
        {
            var position = SamplePoint(end);

            foreach (var other in segments)
            {
                if (!Valid(other)) continue;
                if (other == this) continue;
                if (!CheckConnectionEnd(position, other)) continue;

                connection.other = other;
            }
        }

        private bool CheckConnectionEnd(Vector3 position, TrackSegment other)
        {
            var closest = other.GetClosestPoint(position, true);
            var otherPosition = other.SamplePoint(closest);

            return (position - otherPosition).magnitude < ConnectionDistance;
        }

        public bool IsOffStartOfTrack(Vector3 position)
        {
            var closest = GetClosestPoint(position, false);
            return closest < 0.0f;
        }

        public TrackSegment GetNextTrackStart() => startConnection.other;

        public bool IsOffEndOfTrack(Vector3 position)
        {
            var closest = GetClosestPoint(position, false);
            return closest > 1.0f;
        }

        public TrackSegment GetNextTrackEnd() => endConnection.other;

        public Transform KnotContainer()
        {
            if (!knotContainer) knotContainer = transform.Find("Knots");
            if (knotContainer) return knotContainer;

            knotContainer = new GameObject("Knots").transform;
            knotContainer.SetParent(transform);

            knotContainer.localPosition = Vector3.zero;
            knotContainer.localRotation = Quaternion.identity;
            knotContainer.localScale = Vector3.one;

            knotContainer.SetAsFirstSibling();

            return knotContainer;
        }

        public void UpdateKnotNames()
        {
            foreach (Transform knot in KnotContainer())
            {
                knot.name = $"Knot.{knot.GetSiblingIndex() + 1}";
            }
        }

        [Serializable]
        public class Connection
        {
            public TrackSegment other;
            public bool active;
        }

        public void AddKnot()
        {
            var container = KnotContainer();
            var knot = new GameObject("Knot").transform;
            knot.SetParent(container);
            knot.SetAsLastSibling();

            if (container.childCount > 1)
            {
                var previous = container.GetChild(container.childCount - 1);
                knot.position = previous.position + previous.forward;
            }

            UpdateKnotNames();
        }

        public Transform this[int index]
        {
            get
            {
                var container = KnotContainer();
                var count = container.childCount;
                index = (index % count + count)  % count;
                return container.GetChild(index);
            }
        }

        public IEnumerator GetEnumerator() => KnotContainer().GetEnumerator();
        IEnumerator<Transform> IEnumerable<Transform>.GetEnumerator() => (IEnumerator<Transform>)KnotContainer().GetEnumerator();

        public static bool Valid(TrackSegment segment)
        {
            if (!segment) return false;
            if (!segment.KnotContainer()) return false;
            if (segment.Count < 2) return false;

            return true;
        }
    }
}