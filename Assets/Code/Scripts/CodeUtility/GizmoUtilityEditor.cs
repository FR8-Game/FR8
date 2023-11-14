#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FR8.Runtime.CodeUtility
{
    public static partial class GizmoUtility
    {
        public static int resolution = 12;
        
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
            
            Handles.DrawPolyLine(point(-radius, radius - halfHeight), point(-radius, halfHeight - radius));
            Handles.DrawPolyLine(point(radius, radius - halfHeight), point(radius, halfHeight - radius));
            
            DrawArc(c0, right, up, 0.0f, 180.0f, radius);
            DrawArc(c1, right, up, 180.0f, 180.0f, radius);
            
            Vector3 point(float x, float y) => position + right * x + up * y;
        }

        public static void LineLoop(params Vector3[] points)
        {
            var newPoints = new Vector3[points.Length + 1];
            for (var i = 0; i < points.Length; i++) newPoints[i] = points[i];
            
            newPoints[^1] = newPoints[0];
            Handles.DrawLines(newPoints);
        }

        public static void DrawCircle(Vector3 center, Vector3 right, Vector3 up, float radius)
        {
            DrawArc(center, right, up, 0.0f, 360.0f, radius);
        }
        
        public static void DrawArc(Vector3 center, Vector3 right, Vector3 up, float arcOffsetDeg, float arcAngleDeg, float radius)
        {
            var arcOffset = arcOffsetDeg * Mathf.Deg2Rad;
            var arcAngle = arcAngleDeg * Mathf.Deg2Rad;

            var points = new List<Vector3>();
            for (var i = 0; i < resolution; i++)
            {
                var a0 = i / (float)resolution * arcAngle + arcOffset;
                var p0 = center + (right * Mathf.Cos(a0) + up * Mathf.Sin(a0)) * radius;
                
                points.Add(p0);
            }
            Handles.DrawPolyLine(points.ToArray());
        }
    }
}
#endif