using System.Collections.Generic;
using FR8.Splines;
using UnityEngine;
using Array = FR8.Utility.Array;

namespace FR8.Track
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrackSegment : MonoBehaviour
    {
        [SerializeField] private bool closedLoop;
        [SerializeField] private int resolution = 100;

        private List<Transform> knots = new();
        private List<Vector3> segments = new();
        private float totalLength;

        public List<Transform> Knots => knots;

        private void Awake()
        {
            Bake();
        }

        private void OnDrawGizmosSelected()
        {
            Bake();
            Gizmos.color = Color.yellow;

            for (var i = 0; i < segments.Count - 1; i++)
            {
                Gizmos.DrawLine(segments[i], segments[i + 1]);
            }
            
            Gizmos.DrawSphere(knots[0].position, 1.0f);
            Gizmos.DrawSphere(knots[^1].position, 1.0f);
            
            Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
            for (var i = 0; i < knots.Count - 1; i++)
            {
                var p0 = knots[i].position;
                var p1 = knots[i + 1].position;

                Gizmos.DrawLine(p0, p1);
            }

            if (closedLoop) Gizmos.DrawLine(knots[^1].position, knots[0].position);
        }

        public void Bake()
        {
            FindKnots();
            segments.Clear();

            for (var i = 0; i < resolution; i++)
            {
                var p0 = i / (resolution - 1.0f);
                segments.Add(Sample(p0));
            }

            if (closedLoop) segments.Add(segments[0]);

            totalLength = 0.0f;
            for (var i = 0; i < segments.Count - 1; i++)
            {
                totalLength += (segments[i] - segments[i + 1]).magnitude;
            }
        }

        public void FindKnots()
        {
            knots.Clear();
            foreach (Transform child in transform)
            {
                knots.Add(child);
            }
        }
        
        public Vector3 GetKnot(int index)
        {
            if (closedLoop) return Array.IndexLoop(knots, index).position;

            if (index == 0)
            {
                var p0 = knots[0].position;
                var p1 = knots[1].position;
                return 2.0f * p0 - p1;
            }

            if (index == knots.Count + 1)
            {
                var p0 = knots[^1].position;
                var p1 = knots[^2].position;
                return 2.0f * p0 - p1;
            }

            return knots[index - 1].position;
        }

        public Vector3 Sample(float t)
        {
            if (knots.Count < 4) return transform.position;

            var knotCount = closedLoop ? knots.Count + 3 : knots.Count + 2;

            t *= knotCount - 3;
            var i0 = Mathf.FloorToInt(t);
            if (i0 >= knotCount - 4) i0 = knotCount - 4;

            var p0 = GetKnot(i0);
            var p1 = GetKnot(i0 + 1);
            var p2 = GetKnot(i0 + 2);
            var p3 = GetKnot(i0 + 3);

            return Spline.CatmullRom.Evaluate(p0, p1, p2, p3, t - i0);
        }

        public Vector3 ByDistance(float distance)
        {
            distance = (distance % totalLength + totalLength) % totalLength;
            for (var i = 0; i < segments.Count - 1; i++)
            {
                var a = segments[i];
                var b = segments[i + 1];
                var segmentLength = (a - b).magnitude;
                if (distance < segmentLength) return Vector3.Lerp(a, b, distance / segmentLength);

                distance -= segmentLength;
            }

            return segments[^1];
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            var ai = 0;
            var bi = 0;
            var d0 = float.MaxValue;
            for (var i = 0; i < segments.Count; i++)
            {
                var d1 = (segments[i] - point).magnitude;
                if (d1 > d0) continue;

                bi = ai;
                ai = i;
                d0 = d1;
            }

            var a = segments[ai];
            var b = segments[bi];

            var v0 = (point - a).normalized;
            var v1 = (b - a).normalized;
            var p = Vector3.Dot(v0, v1);
            return Vector3.Lerp(a, b, p);
        }

        public void OnValidate()
        {
            resolution = Mathf.Max(resolution, knots.Count);
            for (var i = 0; i < knots.Count; i++)
            {
                knots[i].name = $"Knot.{i}";
            }
        }
    }
}