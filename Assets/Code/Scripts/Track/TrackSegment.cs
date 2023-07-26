using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using FR8.Splines;
using UnityEngine;
using Array = FR8.Utility.Array;

#if UNITY_EDITOR
#endif

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

        public static readonly Spline.SplineProfile SplineProfile = Spline.CatmullRom;
        
        private void Awake()
        {
            Bake();
        }

        private void OnDrawGizmos()
        {
            Bake();

            Gizmos.color = Color.yellow;

            for (var i = 0; i < segments.Count - 1; i++)
            {
                Gizmos.DrawLine(segments[i], segments[i + 1]);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Bake();

            if (knots.Count < 4) return;
             
            Gizmos.DrawWireSphere(knots[0].position, 2.0f);
            Gizmos.DrawWireSphere(knots[^1].position, 2.0f);

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

            var junction = transform.parent ? transform.parent.GetComponent<TrackJunction>() : null;
            if (!junction) return;

            junction.Setup();
            knots.AddRange(junction.BranchKnots);
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

            return SplineProfile(p0, p1, p2, p3).EvaluatePoint(t - i0);
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