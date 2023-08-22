using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FR8Runtime.CodeUtility
{
    public static class GizmoExtras
    {
        public static int resolution = 32;
        
#if UNITY_EDITOR
        public static Action<Vector3, Vector3, Color, Matrix4x4> drawLineAction = (a, b, color, matrix) =>
        {
            var pColor = Handles.color;
            var pMatrix = Handles.matrix;
            var pZTest = Handles.zTest;
            
            Handles.color = color;
            Handles.matrix = matrix;
            Handles.zTest = CompareFunction.Less;
            
            Handles.DrawAAPolyLine(2.0f, a, b);

            Handles.color = pColor;
            Handles.matrix = pMatrix;
            Handles.zTest = pZTest;
        };
#else
        public static Action<Vector3, Vector3> drawLineAction = null;
#endif

        public static void DrawLine(Vector3 a, Vector3 b) => drawLineAction?.Invoke(a, b, Gizmos.color, Gizmos.matrix);

        public static void DrawCapsule(Vector3 center, UnityEngine.Quaternion orientation, float height, float radius)
        {
            var right = orientation * Vector3.right;
            var up = orientation * Vector3.up;
            var forward = orientation * Vector3.forward;

            var halfHeight = height / 2.0f;
            
            DrawDiscoRectangle(center, orientation, height, radius);
            DrawDiscoRectangle(center, orientation * UnityEngine.Quaternion.Euler(0.0f, 90.0f, 0.0f), height, radius);
            
            DrawCircle(center + up * (halfHeight - radius), right, forward, radius);
            DrawCircle(center - up * (halfHeight - radius), right, forward, radius);
        }

        public static void DrawDiscoRectangle(Vector3 position, UnityEngine.Quaternion orientation, float height, float radius)
        {
            var right = orientation * Vector3.right;
            var up = orientation * Vector3.up;

            var halfHeight = height / 2.0f;

            var c0 = position + Vector3.up * (halfHeight - radius);
            var c1 = position - Vector3.up * (halfHeight - radius);
            
            DrawLine(point(-radius, radius - halfHeight), point(-radius, halfHeight - radius));
            DrawLine(point(radius, radius - halfHeight), point(radius, halfHeight - radius));
            
            DrawArc(c0, right, up, 0.0f, 180.0f, radius);
            DrawArc(c1, right, up, 180.0f, 180.0f, radius);
            
            Vector3 point(float x, float y) => position + right * x + up * y;
        }

        public static void LineLoop(params Vector3[] points)
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                DrawLine(a, b);
            }

            DrawLine(points[^1], points[0]);
        }

        public static void DrawCircle(Vector3 center, Vector3 right, Vector3 up, float radius)
        {
            DrawArc(center, right, up, 0.0f, 360.0f, radius);
        }
        
        public static void DrawArc(Vector3 center, Vector3 right, Vector3 up, float arcOffsetDeg, float arcAngleDeg, float radius)
        {
            var arcOffset = arcOffsetDeg * Mathf.Deg2Rad;
            var arcAngle = arcAngleDeg * Mathf.Deg2Rad;

            for (var i = 0; i < resolution; i++)
            {
                var a0 = i / (float)resolution * arcAngle + arcOffset;
                var a1 = (i + 1.0f) / resolution * arcAngle + arcOffset;

                var p0 = center + (right * Mathf.Cos(a0) + up * Mathf.Sin(a0)) * radius;
                var p1 = center + (right * Mathf.Cos(a1) + up * Mathf.Sin(a1)) * radius;

                DrawLine(p0, p1);
            }
        }
    }
}