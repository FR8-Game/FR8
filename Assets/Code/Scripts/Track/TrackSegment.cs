using System;
using System.Collections.Generic;
using FR8.Splines;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace FR8.Track
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrackSegment : MonoBehaviour
    {
        private const float MergeDistance = 10.0f;

        [SerializeField] private List<Vector3> knots = new();
        [SerializeField] private List<Connection> startConnections = new();
        [SerializeField] private List<Connection> endConnections = new();
        [SerializeField] private int resolution = 100;

        private float totalLength;

        public List<Vector3> Knots => knots;

        public static readonly Spline.SplineProfile SplineProfile = Spline.CatmullRom;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            for (var i = 0; i < resolution; i++)
            {
                var p0 = i / (float)resolution;
                var p1 = (i + 1.0f) / resolution;

                Gizmos.color = new Color(p0, 1.0f - p0, 0.0f, 1.0f);
                Gizmos.DrawLine(SamplePoint(p0), SamplePoint(p1));
            }

            Gizmos.color = Color.white;
            for (var i = 1; i < knots.Count - 1; i++)
            {
                var knot = knots[i];
                Gizmos.DrawSphere(knot, 1.0f);
            }

            Gizmos.DrawCube(knots[0], Vector3.one * 2.0f * 5.0f);
            Gizmos.DrawSphere(knots[^1], 5.0f);

            Vector3 getOtherKnot(Connection connection, int offset)
            {
                return connection.connectedSegment.Knots[connection.type switch
                {
                    Connection.ConnectionType.End => ^(1 + offset),
                    Connection.ConnectionType.Start => offset,
                    _ => throw new ArgumentOutOfRangeException()
                }];
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
            for (var i = 0; i < knots.Count - 1; i++)
            {
                var p0 = Knots[i];
                var p1 = Knots[i + 1];

                Gizmos.DrawLine(p0, p1);
            }
        }

        public Vector3 Knot(int i)
        {
            if (i == 0) return 2.0f * knots[0] - knots[1];
            if (i == knots.Count + 1) return 2.0f * knots[^1] - knots[^2];
            return knots[i - 1];
        }

        public int KnotCount() => knots.Count + 2;

        public Vector3 SamplePoint(float t) => Sample(t, (spline, t) => spline.EvaluatePoint(t));
        public Vector3 SampleTangent(float t) => Sample(t, (spline, t) => spline.EvaluateVelocity(t).normalized);

        public T Sample<T>(float t, Func<Spline, float, T> callback) => Sample(Knot, KnotCount(), t, callback);

        private static T Sample<T>(Func<int, Vector3> knots, int knotCount, float t, Func<Spline, float, T> callback)
        {
            if (knotCount < 4) return default;

            if (t < 0.0f || t > 1.0f) Debug.Log(t);

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
        }

        public float GetClosestPoint(Vector3 point)
        {
            var res0 = 0.5f;
            for (var i = 0; i < 4; i++)
            {
                var res1 = 0.0f;
                var distance = float.MaxValue;
                for (var j = 0; j < 10; j++)
                {
                    var p0 = j / 9.0f;
                    var p1 = res0 + (p0 - 0.5f) / Mathf.Pow(10.0f, i);

                    var other = SamplePoint(p1);
                    var dist2 = (other - point).sqrMagnitude;
                    if (dist2 > distance) continue;

                    distance = dist2;
                    res1 = p1;
                }
                res0 = res1;
            }

            return res0;
        }

        [System.Serializable]
        public class Connection
        {
            public TrackSegment connectedSegment;
            public ConnectionType type;

            public enum ConnectionType
            {
                Start,
                End,
            }
        }
    }
}