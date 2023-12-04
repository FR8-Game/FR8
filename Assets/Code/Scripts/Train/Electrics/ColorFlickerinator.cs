using UnityEngine;

namespace FR8.Runtime.Train.Electrics
{
    [System.Serializable]
    public class ColorFlickerinator
    {
        public Color baseColor = Color.white;
        public float flickerFrequency = 25.0f;
        public float flickerAmplitude = 0.06f;

        public ColorFlickerinator() { }

        public ColorFlickerinator(Color baseColor) { this.baseColor = baseColor; }

        public ColorFlickerinator(Color baseColor, float flickerFrequency, float flickerAmplitude)
        {
            this.baseColor = baseColor;
            this.flickerFrequency = flickerFrequency;
            this.flickerAmplitude = flickerAmplitude;
        }

        public static float Flicker(float frequency, float amplitude)
        {
            return 1.0f + (Noise(Time.time * frequency) * 2.0f - 1.0f) * amplitude;
        }
        
        public static Color Flicker(Color color, float frequency, float amplitude)
        {
            var v = Flicker(frequency, amplitude);
            color.r *= v;
            color.g *= v;
            color.b *= v;

            return color;
        }

        public static implicit operator Color(ColorFlickerinator flickerinator)
        {
            if (flickerinator == null) return Color.black;
            return Flicker(flickerinator.baseColor, flickerinator.flickerFrequency, flickerinator.flickerAmplitude);
        }

        private static float Noise(float x)
        {
            var x0 = Mathf.FloorToInt(x);
            var x1 = x0 + 1;
            var xp = x - x0;

            var n0 = Noise(x0);
            var n1 = Noise(x1);
            return Mathf.Lerp(n0, n1, xp);
        }

        private static float Noise(int x)
        {
            var v = Mathf.Sin(x);
            v *= 12.9898f;
            v = Mathf.Sin(v) * 143758.5453f;
            v = (v % 1.0f + 1.0f) % 1.0f;
            return v;
        }

        public static implicit operator ColorFlickerinator(Color color) => new(color);
    }
}