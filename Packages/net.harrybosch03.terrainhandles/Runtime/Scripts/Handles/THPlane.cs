using UnityEngine;

namespace TerrainHandles.Handles
{
    public class THPlane : TerrainHandle
    {
        public override float AffectedSize => float.MaxValue;

        public override float Apply(float w, Vector3 point, TerrainData data)
        {
            return Blend(w, Vector3.Dot(Position - point, Up));
        }
    }
}
