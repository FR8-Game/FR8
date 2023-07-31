using System;
using System.Collections.Generic;
using FR8.Train.Splines;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FR8.Train.Track
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrackSegment : MonoBehaviour
    {
        [SerializeField] private List<Vector3> knots = new();
        [SerializeField] private int resolution = 100;
        [SerializeField] private bool closedLoop;

        [SerializeField] private EndConnection startConnection;
        [SerializeField] private EndConnection endConnection;

        private float totalLength;

        private Dictionary<TrainMovement, Vector3> trainMetadata = new();

        public List<Vector3> Knots => knots;

        public static readonly Spline.SplineProfile SplineProfile = Spline.CatmullRom;

        private void FixedUpdate()
        {
            var trains = FindObjectsOfType<TrainMovement>();

            UpdateConnection(trains, startConnection);
            UpdateConnection(trains, endConnection);
            UpdateTrainMetadata(trains);
        }

        private void UpdateTrainMetadata(TrainMovement[] trains)
        {
            foreach (var train in trains)
            {
                if (!trainMetadata.ContainsKey(train)) trainMetadata.Add(train, train.Rigidbody.position);
                else trainMetadata[train] = train.Rigidbody.position;
            }
        }

        private void UpdateConnection(TrainMovement[] trains, EndConnection connection)
        {
            if (!connection) return;
            if (!connection.connectionActive) return;

            foreach (var train in trains)
            {
                if (train.Walker.CurrentSegment != connection.segment) continue;
                if (!trainMetadata.ContainsKey(train)) continue;

                var knotPercent = GetKnotPercent(connection.knotIndex);

                var lastSign = (GetClosestPoint(trainMetadata[train]) - knotPercent) > 0.5f;
                var sign = (GetClosestPoint(train.Rigidbody.position) - knotPercent) > 0.5f;
                var switchSign = connection.handleScale > 0.0f;

                if (sign == switchSign && lastSign != switchSign)
                {
                    train.Walker.CurrentSegment = this;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Handles.color = Color.yellow;

            for (var i = 0; i < resolution; i++)
            {
                var p0 = i / (float)resolution;
                var p1 = (i + 1.0f) / resolution;

                Handles.color = new Color(p0, 1.0f - p0, 0.0f, 1.0f);
                Handles.DrawAAPolyLine(SamplePoint(p0), SamplePoint(p1));
            }

            Handles.color = Color.white;
            Gizmos.color = Color.white;
            for (var i = 1; i < knots.Count - 1; i++)
            {
                var knot = knots[i];
                Gizmos.DrawWireSphere(knot, 0.4f);
            }

            Gizmos.DrawWireCube(knots[0], Vector3.one * 2.0f * 2.0f);
            Gizmos.DrawWireSphere(knots[^1], 2.0f);
        }

        private void OnDrawGizmosSelected()
        {
            Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
            for (var i = 0; i < KnotCount(); i++)
            {
                var p0 = Knot(i);

                Handles.color = new Color(0.4f, 1.0f, 0.2f, 1.0f);
                if (Physics.Raycast(p0, Vector3.down, out var hit) && hit.distance > 0.1f)
                {
                    Handles.DrawAAPolyLine(p0, hit.point);
                    Handles.DrawWireArc(hit.point, Vector3.up, Vector3.right, 360.0f, 2.0f);
                }

                if (Physics.Raycast(p0, Vector3.up, out hit) && hit.distance > 0.1f)
                {
                    Handles.DrawAAPolyLine(p0, hit.point);
                    Handles.DrawWireArc(hit.point, Vector3.up, Vector3.right, 360.0f, 2.0f);
                }

                if (i == KnotCount() - 1) continue;

                var p1 = Knot(i + 1);
                Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
                Handles.DrawAAPolyLine(p0, p1);
            }

            Gizmos.color = Handles.color;
            if (startConnection)
            {
                Gizmos.DrawSphere(Knot(0), 0.2f);
                Gizmos.DrawSphere(Knot(1), 0.2f);
                Gizmos.DrawSphere(Knot(2), 0.2f);
            }

            if (endConnection)
            {
                Gizmos.DrawSphere(Knot(KnotCount() - 1), 0.2f);
                Gizmos.DrawSphere(Knot(KnotCount() - 2), 0.2f);
                Gizmos.DrawSphere(Knot(KnotCount() - 3), 0.2f);
            }
        }

        public Vector3 Knot(int i)
        {
            if (closedLoop)
            {
                return transform.TransformPoint(knots[(i % knots.Count + knots.Count) % knots.Count]);
            }

            if (startConnection && i == 0) return startConnection.KnotForwardTangent;
            if (startConnection && i == 1) return startConnection.Knot;
            if (startConnection && i == 2) return startConnection.KnotBackTangent;

            var end = startConnection ? knots.Count + EndConnection.AdditionalKnotsPerConnection : knots.Count;

            if (endConnection && i == end) return endConnection.KnotForwardTangent;
            if (endConnection && i == end + 1) return endConnection.Knot;
            if (endConnection && i == end + 2) return endConnection.KnotBackTangent;

            return transform.TransformPoint(knots[startConnection ? i - EndConnection.AdditionalKnotsPerConnection : i]);
        }

        public Vector3 KnotVelocity(int index) => SampleVelocity(GetKnotPercent(index));

        public int KnotCount()
        {
            if (closedLoop) return knots.Count + 3;

            var c = knots.Count;
            if (startConnection) c += EndConnection.AdditionalKnotsPerConnection;
            if (endConnection) c += EndConnection.AdditionalKnotsPerConnection;

            return c;
        }

        public Vector3 SamplePoint(float t) => Sample(t, (spline, t) => spline.EvaluatePoint(t));
        public Vector3 SampleTangent(float t) => SampleVelocity(t).normalized;
        public Vector3 SampleVelocity(float t) => Sample(t, (spline, t) => spline.EvaluateVelocity(t));

        public T Sample<T>(float t, Func<Spline, float, T> callback) => Sample(Knot, KnotCount(), closedLoop ? t % 1.0f : Mathf.Clamp01(t), callback);

        private static T Sample<T>(Func<int, Vector3> knots, int knotCount, float t, Func<Spline, float, T> callback)
        {
            if (knotCount < 4) return default;

            t *= knotCount - 3;
            var i0 = Mathf.FloorToInt(t);
            if (i0 >= knotCount - 4) i0 = knotCount - 4;

            var p0 = knots(i0 + 0);
            var p1 = knots(i0 + 1);
            var p2 = knots(i0 + 2);
            var p3 = knots(i0 + 3);

            return callback(SplineProfile(p0, p1, p2, p3), t - i0);
        }

        public void OnValidate()
        {
            resolution = Mathf.Max(resolution, knots.Count);
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).name = $"Knot.{i}";
            }

            startConnection.OnValidate();
            endConnection.OnValidate();
        }

        public float GetClosestPoint(Vector3 point, bool debugDraw = false)
        {
            var colors = new[]
            {
                Color.red,
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                Color.magenta,
            };

            var t0 = 0.0f;
            var bestDistanceSquares = float.MaxValue;

            for (var i = 0; i < resolution; i++)
            {
                var p = closedLoop ? (float)i / resolution : i / (resolution - 1.0f);

                var other = SamplePoint(p);
                var otherDistanceSquared = (other - point).magnitude;

                if (otherDistanceSquared > bestDistanceSquares) continue;

                t0 = p;
                bestDistanceSquares = otherDistanceSquared;
            }

            var t1 = t0 + 1.0f / resolution;

            var point0 = SamplePoint(t0);
            var point1 = SamplePoint(t1);

            if (debugDraw) Debug.DrawRay(point0, (point - point0) * 0.5f, colors[0]);
            if (debugDraw) Debug.DrawRay(point1, (point - point1) * 0.5f, colors[0]);

            var v0 = point1 - point0;
            var v1 = point - point0;

            var dot = Vector3.Dot(v0.normalized, v1) / v0.magnitude;

            return t0 + dot / resolution;
        }

        public float GetKnotPercent(int index)
        {
            return (index - 1.0f) / (KnotCount() - 3.0f);
        }

        public int GetKnotIndex(float t)
        {
            t = Mathf.Clamp(t, 0.0f, 0.99999f);

            var i = Mathf.FloorToInt(t * (KnotCount() - 2)) + 1;
            return i;
        }

        [Serializable]
        public class EndConnection
        {
            public const int AdditionalKnotsPerConnection = 3;

            public TrackSegment segment;
            public int knotIndex;
            public float handleScale = 1.0f;
            public bool connectionActive;

            public Vector3 Knot => segment.Knot(knotIndex);
            public Vector3 KnotForwardTangent => Knot + Velocity;
            public Vector3 KnotBackTangent => Knot - Velocity;
            public Vector3 Velocity => segment.KnotVelocity(knotIndex) * handleScale;

            public void OnValidate()
            {
                if (segment)
                {
                    if (!segment.closedLoop) knotIndex = Mathf.Clamp(knotIndex, 0, segment.knots.Count - 1);
                }
            }

            public static implicit operator bool(EndConnection connection)
            {
                if (connection == null) return false;
                if (!connection.segment) return false;
                if (connection.handleScale == 0.0f) return false;
                if (connection.knotIndex < 1 || connection.knotIndex > connection.segment.knots.Count - 2) return true;

                return true;
            }
        }
    }
}