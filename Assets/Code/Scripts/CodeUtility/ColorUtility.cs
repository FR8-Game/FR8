using UnityEngine;

namespace FR8Runtime.CodeUtility
{
    public static class ColorUtility
    {
        public static Color Invert(Color color, bool invertAlpha = false) => new()
        {
            r = 1.0f - color.r,
            g = 1.0f - color.g,
            b = 1.0f - color.b,
            a = invertAlpha ? 1.0f - color.a : color.a
        };
    }
}