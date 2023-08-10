using UnityEngine;

namespace FR8
{
    public static partial class Utility
    {
        public static class Quaternion
        {
            public static (float, Vector3) ToAngleAxis(UnityEngine.Quaternion q)
            {
                q.ToAngleAxis(out var angle, out var axis);
                if (!float.IsFinite(axis.magnitude)) return (0.0f, Vector3.up);

                if (Mathf.Abs(angle) > 180.0f) angle = 360.0f - angle;

                return (angle, axis.normalized);
            }
        }
    }
}