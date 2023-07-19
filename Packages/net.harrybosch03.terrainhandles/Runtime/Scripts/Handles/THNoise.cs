using UnityEngine;

namespace TerrainHandles.Handles
{
    public class THNoise : TerrainHandle
    {
        [SerializeField] private float radius;
        [SerializeField] private float size;
        [SerializeField] private float amplitude;
        [SerializeField] private int octaves;
        [SerializeField] private float lacunarity;
        [SerializeField] private float persistence;
        [SerializeField] private float lift;
        [SerializeField][Range(0.0f, 1.0f)] private float hardness;

        public override float AffectedSize => radius;
        public override float Apply(float w, Vector3 point, TerrainData data)
        {
            var vector = WorldToLocal(point);
            var distance = new Vector2(vector.x, vector.z).magnitude / radius;
            var blend = Mathf.Clamp01((1.0f - distance) / (1.0f - hardness * 0.999f));

            var noise = GetNoise(point) * 2.0f - 1.0f;

            return w + (noise + lift) * blend * amplitude;
        }

        private float GetNoise(Vector3 point)
        {
            if (octaves < 1) return 0.0f;
            
            var uv = new Vector2(point.x, point.z);

            var noise = 0.0f;
            var max = 0.0f;

            for (var i = 0; i < octaves; i++)
            {
                var frequency = Mathf.Pow(lacunarity, i) / size;
                var amplitude = Mathf.Pow(persistence, i);

                noise += Mathf.PerlinNoise(uv.x * frequency, uv.y * frequency) * amplitude;
                max += amplitude;
            }
            
            return noise / max;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            octaves = Mathf.Max(1, octaves);
        }
    }
}
