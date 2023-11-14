using UnityEngine;

namespace FR8.Runtime.CodeUtility
{
    public static class Math
    {
        public const float Sqrt2 = 1.4142135623730950488016887242097f;

        public static float Remap(float lowerIn, float upperIn, float lowerOut, float upperOut, float value)
        {
            var p = (value - lowerIn) / (upperIn - lowerIn);
            return p * (upperOut - lowerOut) + lowerOut;
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 t)
        {
            var v1 = t - a;
            var v2 = b - a;

            return Vector3.Dot(v1, v2.normalized) / v2.magnitude;
        }
    }
}