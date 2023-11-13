using System;
using UnityEngine;

namespace FR8.Runtime.Shapes
{
    public class SphereShape : Shape
    {
        public float radius;
        
        public override bool ContainsPoint(Vector3 point)
        {
            var localPoint = transform.InverseTransformPoint(point);
            return localPoint.sqrMagnitude < radius * radius;
        }

        public override void Draw(Action<Vector3, Vector3> drawLine)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }
    }
}
