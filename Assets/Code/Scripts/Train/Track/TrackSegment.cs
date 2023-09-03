using System;
using System.Collections;
using System.Collections.Generic;
using FR8Runtime.Train.Splines;
using UnityEditor;
using UnityEngine;

namespace FR8Runtime.Train.Track
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrackSegment : MonoBehaviour, IEnumerable<Transform>
    {
        public const float ConnectionDistance = 3.0f;
        public static readonly Spline.SplineProfile SplineProfile = Spline.Cubic;

        [SerializeField] [HideInInspector] private List<Vector3> knots;
        public TrackJunction junctionPrefab;

        [Space]
        [SerializeField] private int resolution = 100;

        [SerializeField] private bool closedLoop;

        private Connection startConnection = new();
        private Connection endConnection = new();

        private float totalLength;

        private Dictionary<TrainCarriage, Vector3> trainMetadata = new();
        private List<Vector3> points;

        public Connection StartConnection => startConnection;
        public Connection EndConnection => endConnection;
        public int Resolution => resolution;
        public int Count => KnotContainer().childCount;

        private void Awake()
        {
            BakePoints();
        }

        private void Start()
        {
            if (startConnection)
            {
                junctionPrefab.SpawnFromPrefab(this, this[0]);
            }

            if (endConnection)
            {
                junctionPrefab.SpawnFromPrefab(this, this[FromEnd(1)]);
            }
        }

        private void FixedUpdate()
        {
            var trains = FindObjectsOfType<TrainCarriage>();

            UpdateConnection(trains, ConnectionType.Start);
            UpdateConnection(trains, ConnectionType.End);
            UpdateTrainMetadata(trains);
        }

        public void DrawGizmos(bool main, Color selectedColor, Color otherColor)
        {
            BakePoints();
            var linePoints = new List<Vector3>();

            foreach (var e in points)
            {
                linePoints.Add(e);
            }

            GizmosDrawLine(main ? selectedColor : otherColor, 1.0f, linePoints.ToArray());
            DrawLineBetweenKnots();
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
        }

        private void GizmosDrawLine(Color color, float width = 1.0f, params Vector3[] points)
        {
#if UNITY_EDITOR
            width *= 4.0f;

            Handles.color = color;
            Handles.DrawPolyLine(points);
#endif
        }

        private void BakePoints()
        {
            points = new List<Vector3>();
            for (var i = 0; i < resolution; i++)
            {
                var p = i / (resolution - 1.0f);
                points.Add(SamplePoint(p));
            }
        }

        private void UpdateTrainMetadata(TrainCarriage[] trains)
        {
            foreach (var train in trains)
            {
                if (!trainMetadata.ContainsKey(train)) trainMetadata.Add(train, train.Rigidbody.position);
                else trainMetadata[train] = train.Rigidbody.position;
            }
        }

        private void UpdateConnection(TrainCarriage[] trains, ConnectionType type)
        {
            var connection = type switch
            {
                ConnectionType.Start => startConnection,
                ConnectionType.End => endConnection,
                _ => throw new ArgumentOutOfRangeException()
            };

            var other = connection.segment;

            if (!connection) return;
            if (connection.connectionActive)
            {
                foreach (var train in trains)
                {
                    if (train.Segment != other) continue;
                    if (!trainMetadata.ContainsKey(train)) continue;

                    TryExplicitJump(other, connection, train);
                }
            }

            foreach (var train in trains)
            {
                if (train.Segment != this) continue;

                TryImplicitJump(type, train, connection);
            }
        }

        private void TryImplicitJump(ConnectionType type, TrainCarriage train, Connection connection)
        {
            var p = GetClosestPoint(train.Rigidbody.position);
            switch (type)
            {
                case ConnectionType.Start:
                {
                    if (p < 0.0f)
                    {
                        train.Segment = connection.segment;
                    }

                    break;
                }
                case ConnectionType.End:
                {
                    if (p > 1.0f)
                    {
                        train.Segment = connection.segment;
                    }

                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void TryExplicitJump(TrackSegment other, Connection connection, TrainCarriage train)
        {
            float difference(float p0, float p1, bool loop)
            {
                var diff = p0 - p1;
                if (!loop) return diff;

                if (diff > 0.5f) diff -= 1.0f;
                if (diff < -0.5f) diff += 1.0f;
                return diff;
            }

            var knotPercent = connection.t;

            var lastSign = difference(other.GetClosestPoint(trainMetadata[train]), knotPercent, other.closedLoop) > 0.0f;
            var d0 = difference(other.GetClosestPoint(train.Rigidbody.position), knotPercent, other.closedLoop);
            var sign = d0 > 0.0f;
            var switchSign = connection.direction > 0.0f;

            if (sign == switchSign && lastSign != switchSign)
            {
                train.Segment = this;
            }
        }

        public Vector3 KnotVelocity(int index) => SampleVelocity(GetKnotPercent(index));

        public int FromEnd(int i) => Count - i;

        public Vector3 SamplePoint(float t) => Sample(t, (spline, t) => spline.EvaluatePoint(t));
        public Vector3 SampleTangent(float t) => SampleVelocity(t).normalized;
        public Vector3 SampleVelocity(float t) => Sample(t, (spline, t) => spline.EvaluateVelocity(t));

        public T Sample<T>(float t, Func<Spline, float, T> callback) => Sample(t, callback, i => (this[i].position, this[i].forward), Count);

        public static T Sample<T>(float t, Func<Spline, float, T> callback, Func<int, (Vector3, Vector3)> list, int count)
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

        public void OnValidate()
        {
            if (!Valid(this)) return;

            OnKnotsChanged();

            var childCount = KnotContainer().childCount;

            resolution = Mathf.Max(resolution, childCount);
            startConnection.OnValidate();
            endConnection.OnValidate();
        }

        [ContextMenu("Update Legacy Knots")]
        public void UpdateLegacyKnots()
        {
            var container = KnotContainer();
            for (var i = 0; i < knots.Count; i++)
            {
                var knot = new GameObject($"Knot.{i + 1}").transform;
                knot.SetParent(container);
                knot.localPosition = knots[i];
                knot.localRotation = Quaternion.identity;
                knot.localScale = Vector3.one;
            }

            knots.Clear();
        }

        public float GetClosestPoint(Vector3 point)
        {
            if (points == null) BakePoints();

            FindClosestPair(point, out var best, out var other);
            return InterpolatePoints(point, best, other);
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
            if (other == points.Count)
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

        public bool ConnectedTo(TrackSegment other)
        {
            if (startConnection.segment == other) return true;
            if (endConnection.segment == other) return true;

            return false;
        }

        public void OnKnotsChanged()
        {
            var segments = FindObjectsOfType<TrackSegment>();

            var container = KnotContainer();
            var childCount = container.childCount;

            updateEnd(0, 1, startConnection);
            updateEnd(childCount - 1, childCount - 2, endConnection);

            void updateEnd(int i, int p, Connection connection)
            {
                connection.segment = null;

                var knot = container.GetChild(i);
                var d0 = knot.position - container.GetChild(p).position;

                foreach (var s in segments)
                {
                    if (!Valid(s)) continue;
                    if (s == this) continue;
                    if (s.ConnectedTo(this)) continue;

                    var t = s.GetClosestPoint(knot.position);
                    var closestPoint = s.SamplePoint(t);
                    if ((knot.position - closestPoint).sqrMagnitude > ConnectionDistance * ConnectionDistance) continue;

                    connection.segment = s;
                    connection.t = t;

                    var d1 = s.SampleVelocity(t);
                    connection.direction = (int)Mathf.Sign(Vector3.Dot(d0, d1));

                    knot.position = closestPoint;

                    var tangent = s.SampleTangent(t);
                    if (Vector3.Dot(tangent, knot.forward) < 0.0f) tangent *= -1.0f;
                    knot.rotation = Quaternion.LookRotation(tangent, Vector3.up);

                    break;
                }
            }
        }

        public Transform KnotContainer()
        {
            var container = transform.Find("Knots");
            if (container) return container;

            container = new GameObject("Knots").transform;
            container.SetParent(transform);

            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            container.localScale = Vector3.one;

            container.SetAsFirstSibling();

            return container;
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
            public const int AdditionalKnotsPerConnection = 3;

            public TrackSegment segment;
            public float t;
            public int direction;
            public bool connectionActive;

            public Vector3 Knot0 => segment.SamplePoint(t) - segment.SampleVelocity(t) * direction / 3.0f;
            public Vector3 Knot1 => segment.SamplePoint(t);
            public Vector3 Knot2 => segment.SamplePoint(t) + segment.SampleVelocity(t) * direction / 3.0f;

            public void OnValidate() { }

            public static implicit operator bool(Connection connection) => connection.segment;
        }

        private enum ConnectionType
        {
            Start,
            End,
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

        public Transform this[int index] => KnotContainer().GetChild(index);

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