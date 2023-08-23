using UnityEngine;

namespace FR8Runtime.CodeUtility
{
    public static class QuaternionUtility
    {
        public static (float, Vector3) ToAngleAxis(UnityEngine.Quaternion q)
        {
            if (q.w < 0.0f) q = UnityEngine.Quaternion.Inverse(q);
            q.ToAngleAxis(out var angle, out var axis);
            if (!float.IsFinite(axis.magnitude)) return (0.0f, Vector3.up);

            if (Mathf.Abs(angle) > 180.0f) angle = 360.0f - angle;

            return (angle, axis.normalized);
        }
    }
}