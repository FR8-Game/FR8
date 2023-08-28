using System;
using System.Collections.Generic;
using FR8Runtime.Train.Splines;
using UnityEditor;
using UnityEngine;

namespace FR8Runtime.Train.Track
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrackSegment : MonoBehaviour
    {
        public const float ConnectionDistance = 3.0f;

        [SerializeField] private List<Vector3> knots;

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

        public static readonly Spline.SplineProfile SplineProfile = Spline.Cubic;

        private void Awake()
        {
            BakePoints();
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

        private void FixedUpdate()
        {
            var trains = FindObjectsOfType<TrainCarriage>();

            UpdateConnection(trains, ConnectionType.Start);
            UpdateConnection(trains, ConnectionType.End);
            UpdateTrainMetadata(trains);
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

        private void OnDrawGizmosSelected()
        {
            BakePoints();

            for (var i = 0; i < points.Count - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var c = (a + b) / 2.0f;

                a = (a - c) * 0.9f + c;
                b = (b - c) * 0.9f + c;

                GizmosDrawLine(a, b, new Color(1.0f, 0.6f, 0.1f, 1.0f));
            }

            DrawLineBetweenKnots();
        }

        private void DrawLineBetweenKnots()
        {
            for (var i = 0; i < KnotCount() - 1; i++)
            {
                var p0 = Knot(i);
                var p1 = Knot(i + 1);
                GizmosDrawLine(p0, p1, new Color(1.0f, 1.0f, 1.0f, 0.4f));
            }
        }

        private void DrawDistanceFromGround(Vector3 p0)
        {
            if (Physics.Raycast(p0, Vector3.down, out var hit))
            {
                var radius = Mathf.Min(2.0f, hit.distance * 2.0f);
                GizmosDrawLine(p0, hit.point, new Color(0.4f, 1.0f, 0.2f, 1.0f));
                Handles.DrawWireArc(hit.point, Vector3.up, Vector3.right, 360.0f, radius);
            }

            if (Physics.Raycast(p0, Vector3.up, out hit))
            {
                var radius = Mathf.Min(2.0f, hit.distance * 20.0f);
                GizmosDrawLine(p0, hit.point, new Color(0.4f, 1.0f, 0.2f, 1.0f));
                Handles.DrawWireArc(hit.point, Vector3.up, Vector3.right, 360.0f, radius);
            }
        }

        private void GizmosDrawLine(Vector3 a, Vector3 b, Color color)
        {
            const float width = 4.0f;

#if UNITY_EDITOR
            Handles.color = color;
            Handles.DrawAAPolyLine(width, a, b);
#endif
        }

        public Vector3 Knot(int i)
        {
            var container = KnotContainer();
            return container.GetChild(i).position;
        }

        public Vector3 KnotVelocity(int index) => SampleVelocity(GetKnotPercent(index));

        public int KnotCount()
        {
            var container = KnotContainer();
            return container.childCount;
        }

        public Vector3 SamplePoint(float t) => Sample(t, (spline, t) => spline.EvaluatePoint(t));
        public Vector3 SampleTangent(float t) => SampleVelocity(t).normalized;
        public Vector3 SampleVelocity(float t) => Sample(t, (spline, t) => spline.EvaluateVelocity(t));

        public T Sample<T>(float t, Func<Spline, float, T> callback)
        {
            t = Mathf.Clamp01(t);
            
            var container = KnotContainer();
            var knotCount = container.childCount;
            
            if (knotCount < 2) return default;

            t *= knotCount - 1;
            var i0 = Mathf.FloorToInt(t);
            if (i0 >= knotCount - 2) i0 = knotCount - 2;

            var k0 = container.GetChild(i0);
            var k1 = container.GetChild(i0 + 1);

            var l = (k1.position - k0.position).magnitude;
            
            var p0 = k0.position;
            var p1 = k0.position + k0.forward * l / 3.0f;
            var p2 = k1.position - k1.forward * l / 3.0f;
            var p3 = k1.position;

            return callback(SplineProfile(p0, p1, p2, p3), t - i0);
        }

        public void OnValidate()
        {
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
            return (index - 1.0f) / (KnotCount() - 2.0f);
        }

        public int GetKnotIndex(float t)
        {
            t = Mathf.Clamp(t, 0.0f, 0.99999f);

            var i = Mathf.FloorToInt(t * (KnotCount() - 2)) + 1;
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
    }
}