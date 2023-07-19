using System;
using UnityEditor;
using UnityEngine;

namespace FR8.Pathing
{
    public class PathCurve : MonoBehaviour
    {
        [SerializeField] private int resolution = 100;

        private Vector3 points;

        public Curve GetCurve() => new(transform);

        private void OnDrawGizmos()
        {
            var curve = GetCurve();
            
            if (resolution <= 0) return; 
            Gizmos.color = Color.yellow;
            var step = 1.0f / resolution;
            for (var p = 0.0f; p < 1.0f - step; p += step)
            {
                var a = curve.Sample(p);
                var b = curve.Sample(p + step);
                Gizmos.DrawLine(a, b);
            }
        }

        private void OnValidate()
        {
            resolution = Mathf.Max(resolution, 1);
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.name = $"Path Point [{i}]";
            }
        }
    }
}