﻿using UnityEngine;

namespace HBCore.Utility
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

        public static Color Gray(float gray, float alpha = 1.0f) => new(gray, gray, gray, alpha);
        public static Color ScaleAlpha(Color color, float alphaScale) => new(color.r, color.g, color.b, color.a * alphaScale);
        public static Color Gray255(int gray, float alpha = 1.0f) => new(gray / 255.0f, gray / 255.0f, gray / 255.0f, alpha);

        public static Color BlendAlpha(Color input, float alpha) => new Color(input.r, input.g, input.b, input.a * alpha);
    }
}