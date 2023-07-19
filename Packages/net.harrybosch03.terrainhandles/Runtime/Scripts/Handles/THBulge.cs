using UnityEngine;

namespace TerrainHandles.Handles
{
    public class THBulge : TerrainHandle
    {
        [SerializeField] private float radius = 5.0f;

        public override float AffectedSize => radius;

        public override float Apply(float w, Vector3 point, TerrainData data)
        {
            var x = WorldToLocal(point).magnitude;
            return Blend(w, radius - x);
        }
    }
}