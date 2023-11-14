using System.Collections.Generic;
using UnityEngine;

namespace FR8.Runtime.CodeUtility
{
    public static class TerrainUtility
    {
        public static Vector3 GetPointOnTerrain(List<UnityEngine.Terrain> terrainList, Vector3 worldPosition)
        {
            foreach (var terrain in terrainList)
            {
                var bounds = terrain.terrainData.bounds;
                bounds.center += terrain.GetPosition();
                bounds.min = new Vector3(bounds.min.x, -10000.0f, bounds.min.z);
                bounds.max = new Vector3(bounds.max.x, 10000.0f, bounds.max.z);

                if (!bounds.Contains(worldPosition)) continue;

                var heightAtKnot = terrain.SampleHeight(worldPosition) + terrain.GetPosition().y;
                return new Vector3(worldPosition.x, heightAtKnot, worldPosition.z);
            }

            return worldPosition;
        }    
    }
}