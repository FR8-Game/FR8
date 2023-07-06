using System.Collections.Generic;
using UnityEngine;

namespace TerrainHandles
{
    [SelectionBase, DisallowMultipleComponent, RequireComponent(typeof(MeshFilter))]
    public sealed class TerrainChunk : MonoBehaviour
    {
        [SerializeField] private Vector2 size;
        [SerializeField] private int resolution;

        private List<Vector3> vertices;
        private List<Vector2> uvs;
        private int[] indices;

        public int Rows => Mathf.FloorToInt(size.x) * resolution;
        public int Columns => Mathf.FloorToInt(size.y) * resolution;
        
        private void Generate()
        {
            var filter = gameObject.GetOrAddComponent<MeshFilter>();
            var mesh = filter.sharedMesh;
            if (!mesh || !mesh.isReadable)
            {
                mesh = filter.sharedMesh = new Mesh();
                mesh.name = "[PROC] Terrain Chunk";
            }
        }

        public void SetIndices()
        {
            var triCount = 2 * Rows * Columns;
            indices = new int[triCount * 3];
            for (var i = 0; i < triCount; i++)
            {
                indices[i * 3] = i;
                indices[i * 3] = i + 1;
            }
        }

        public void AppendPoint(Vector3 point)
        {
            vertices.Add(point);
            uvs.Add(new Vector2(point.x, point.z) / size);
        }

        public Vector3 GetHeight(Vector3 point)
        {
            point.y = 0.0f;
            point.y = Mathf.Exp(-point.sqrMagnitude);
            return point;
        }
    }
}
