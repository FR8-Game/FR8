using System.Collections.Generic;
using System.Linq;
using TerrainHandles.Handles;
using UnityEngine;

namespace TerrainHandles
{
    public class TerrainData
    {
        private List<TerrainHandle> handles;
        private List<Chunk> chunks;

        public List<Chunk> Chunks => chunks;
        
        public TerrainData()
        {
            handles = new List<TerrainHandle>(Object.FindObjectsOfType<TerrainHandle>().OrderBy(e => e.transform.GetSiblingIndex()));
            chunks = new List<Chunk>(Object.FindObjectsOfType<Chunk>());
            
            foreach (var handle in handles)
            {
                handle.Prepare();
            }
        }
        
        public float AtPoint(Vector3 point)
        {
            var v = float.MinValue;
            foreach (var handle in handles)
            {
                v = handle.Apply(v, point, this);
            }
            
            return v;
        }
    }
}