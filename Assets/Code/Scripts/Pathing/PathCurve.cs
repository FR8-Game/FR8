using System;
using UnityEngine;

namespace FR8.Pathing
{
    public class PathCurve : MonoBehaviour
    {
        [SerializeField] private int gizmoResolution = 100;
        
        public Vector3 Sample(float p)
        {
            var i = Mathf.FloorToInt(p * transform.childCount);
            var j = i + 1;

            if (i >= transform.childCount) return transform.position;
            if (j >= transform.childCount) return transform.GetChild(i).position;

            var t = p * transform.childCount - i;

            return Interpolate(transform.GetChild(i), transform.GetChild(j), t);
        }

        public Vector3 Interpolate(Transform a, Transform b, float t)
        {
            var l = (b.position - a.position).magnitude;

            return Interpolate(new[]
            {
                a.position,
                a.position + a.forward * l / 3.0f * a.localScale.z,
                b.position - b.forward * l / 3.0f * b.localScale.z,
                b.position,
            }, t);
        }

        public static Vector3 Interpolate(Vector3[] points, float t)
        {
            while (true)
            {
                var res = new Vector3[points.Length - 1];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i] = Vector3.Lerp(points[i], points[i + 1], t);
                }

                if (res.Length == 1) return res[0];
                points = res;
            }
        }

        private void OnDrawGizmos()
        {
            if (gizmoResolution <= 0) return; 
            Gizmos.color = Color.yellow;
            var step = 1.0f / gizmoResolution;
            for (var p = 0.0f; p < 1.0f - step; p += step)
            {
                var a = Sample(p);
                var b = Sample(p + step);
                Gizmos.DrawLine(a, b);
            }
        }

        private void OnValidate()
        {
            gizmoResolution = Mathf.Max(gizmoResolution, 1);
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.name = $"Path Point [{i}]";
            }
        }
    }
}