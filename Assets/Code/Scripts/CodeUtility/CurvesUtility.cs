using UnityEngine;

namespace FR8.Runtime.CodeUtility
{
    public static class CurvesUtility
    {
        public static float SmootherStep(float x) => x * x * x * (3.0f * x * (2.0f * x - 5.0f) + 10.0f);

        public static float Bounce(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1.0f;

            return 1.0f + c3 * Mathf.Pow(x - 1.0f, 3.0f) + c1 * Mathf.Pow(x - 1.0f, 2.0f);
        }
    }
}