using System.Collections.Generic;
using UnityEngine;

namespace FR8Runtime.Level
{
    [ExecuteAlways]
    public class WindSettings : MonoBehaviour
    {
        public Vector3 force;
        [Range(0.0f, 2.0f)] public float noiseStrength;
        public float noiseFrequency;
        public int octaves;
        public float lacunarity = 2.0f;
        public float persistence = 0.5f;

        private static List<WindSettings> all = new();
        
        private void OnEnable()
        {
            all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        public Vector3 GetForce() => GetForce(Time.time);
        public Vector3 GetForce(float t)
        {
            var noise = GetNoise(t);

            return force * noise;
        }

        public float GetNoise(float t)
        {
            var noise = 0.0f;
            var max = 0.0f;
            for (var i = 0; i < octaves; i++)
            {
                var frequency = Mathf.Pow(lacunarity, i) / noiseFrequency;
                var amplitude = Mathf.Pow(persistence, i);

                max += amplitude;
                noise += Mathf.PerlinNoise(t * frequency, 0.0f) * amplitude;
            }

            noise /= max;
            return Mathf.Clamp(1.0f + (noise * 2.0f - 1.0f) * noiseStrength, 0.0f, 2.0f);
        }

        public static Vector3 GetWindForce(Vector3 position)
        {
            var res = Vector3.zero;
            foreach (var e in all)
            {
                res += e.GetForce();
            }
            return res;
        }

        private void OnValidate()
        {
            octaves = Mathf.Clamp(octaves, 1, 16);
            noiseFrequency = Mathf.Max(0.0f, noiseFrequency);
        }
    }
}