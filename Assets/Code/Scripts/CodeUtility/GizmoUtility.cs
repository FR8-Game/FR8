#if !UNITY_EDITOR
using UnityEngine;

namespace FR8Runtime.CodeUtility
{
    public static partial class GizmoUtility
    {
        public static void DrawCapsule(Vector3 center, UnityEngine.Quaternion orientation, float height, float radius) { }
        public static void DrawDiscoRectangle(Vector3 position, UnityEngine.Quaternion orientation, float height, float radius) { }
        public static void LineLoop(params Vector3[] points) { }
        public static void DrawCircle(Vector3 center, Vector3 right, Vector3 up, float radius) { }
        public static void DrawArc(Vector3 center, Vector3 right, Vector3 up, float arcOffsetDeg, float arcAngleDeg, float radius) { }
    }
}
#endif
