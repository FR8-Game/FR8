using System;
using UnityEngine;

namespace FR8.Runtime.Shapes
{
    public class BoxShape : Shape
    {
        public Vector3 center;
        public Vector3 size = Vector3.one;

        public override bool ContainsPoint(Vector3 point)
        {
            var localPoint = transform.InverseTransformPoint(point);

            var min = center - size / 2.0f;
            var max = center + size / 2.0f;

            if (!checkAxis(0)) return false;
            if (!checkAxis(1)) return false;
            if (!checkAxis(2)) return false;

            return true;
            
            bool checkAxis(int i) => localPoint[i] >= min[i] && localPoint[i] <= max[i];
        }

        public override void Draw(Action<Vector3, Vector3> drawLine)
        {
            var points = new[]
            {
                point(-1, -1, -1),
                point( 1, -1, -1),
                point( 1, -1,  1),
                point(-1, -1,  1),
                
                point(-1,  1, -1),
                point( 1,  1, -1),
                point( 1,  1,  1),
                point(-1,  1,  1),
            };

            line(0, 1);
            line(1, 2);
            line(2, 3);
            line(3, 0);
            
            line(4, 5);
            line(5, 6);
            line(6, 7);
            line(7, 4);
            
            line(0, 4);
            line(1, 5);
            line(2, 6);
            line(3, 7);
            
            void line(int i0, int i1) => drawLine(points[i0], points[i1]);
            Vector3 point(float x, float y, float z) => new(center.x + size.x * 0.5f * x, center.y + size.y * 0.5f * y, center.z + size.z * 0.5f * z);
        }

        protected override void OnDrawGizmosSelected() { }
    }
}