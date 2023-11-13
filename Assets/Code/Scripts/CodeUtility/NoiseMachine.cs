using UnityEngine;

namespace FR8.Runtime.CodeUtility
{
    [System.Serializable]
    public class NoiseMachine
    {
        public float offset = 0.0f;
        public float amplitude = 1.0f;
        public float frequency = 10.0f;
        public int octaves = 4;
        public float lacunarity = 2.0f;
        public float persistence = 0.5f;

        public float Sample(float x, float y)
        {
            OnValidate();

            var val = 0.0f;
            var max = 0.0f;

            for (var i = 0; i < octaves; i++)
            {
                var f = frequency * Mathf.Pow(lacunarity, i);
                var a = Mathf.Pow(persistence, i);

                max += a;
                val += Mathf.PerlinNoise(f * x, f * y) * a;
            }

            return offset + amplitude * (val / max);
        }

        public void OnValidate()
        {
            frequency = Mathf.Max(0.0f, frequency);
            octaves = Mathf.Max(1, octaves);
            lacunarity = Mathf.Max(0.0f, lacunarity);
            persistence = Mathf.Max(0.0f, persistence);
        }
    }
}