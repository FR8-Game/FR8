using System.Collections.Generic;
using UnityEngine;

namespace FR8.Train.Track
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrackSegment))]
    public sealed class TrackModel : MonoBehaviour
    {
        private const int nextPointSubResolution = 10;

        [SerializeField] private Mesh baseMesh;
        [SerializeField] private Material material;
        [SerializeField] private bool optimize;
        [SerializeField] private int segmentsPerSplit = 15;

        private void Awake()
        {
            BakeMesh();
        }

        public void Clear()
        {
            var rendererContainer = transform.Find("Renderers");
            if (rendererContainer) DestroyImmediate(rendererContainer.gameObject);
        }
        
        public void BakeMesh()
        {
            Clear();
            
            var rendererContainer = new GameObject("Renderers").transform;
            rendererContainer.SetParent(transform);
            rendererContainer.localPosition = Vector3.zero;
            rendererContainer.localRotation = Quaternion.identity;

            var segment = GetComponent<TrackSegment>();
            
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var indices = new List<int>();
            var uvs = new List<Vector2>();

            var startPoint = 0.0f;
            var endPoint = 0.0f;
            var segmentSize = baseMesh.bounds.size.z;

            var rnd = new System.Random(gameObject.GetInstanceID());
            var vOffset = (float)rnd.NextDouble();
            vOffset *= 0.01f;

            var index = 0;

            while (endPoint < 1.0f)
            {
                if (index != 0 && index % segmentsPerSplit == 0)
                {
                    SplitMesh(vertices, normals, indices, uvs, rendererContainer);
                }
                
                endPoint = FindNextPoint(segment, startPoint, segmentSize);

                var indexBase = vertices.Count;

                foreach (var v0 in baseMesh.vertices)
                {
                    var p2 = Mathf.Lerp(startPoint, endPoint, Mathf.InverseLerp(baseMesh.bounds.min.z, baseMesh.bounds.max.z, v0.z));

                    var t = segment.SamplePoint(p2);
                    var r = Quaternion.LookRotation(segment.SampleTangent(p2));
                    var v2 = r * new Vector3(v0.x, v0.y, 0.0f) + t;

                    vertices.Add(transform.InverseTransformPoint(v2 + Vector3.up * vOffset));
                }

                foreach (var n in baseMesh.normals)
                {
                    normals.Add(n);
                }

                foreach (var t in baseMesh.triangles)
                {
                    indices.Add(indexBase + t);
                }

                foreach (var uv in baseMesh.uv)
                {
                    uvs.Add(uv);
                }

                startPoint = endPoint;
                index++;
            }
            
            SplitMesh(vertices, normals, indices, uvs, rendererContainer);
        }

        private void SplitMesh(List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs, Transform rendererContainer)
        {
            var filter = new GameObject().AddComponent<MeshFilter>();
            var renderer = filter.gameObject.AddComponent<MeshRenderer>();
            var collider = filter.gameObject.AddComponent<MeshCollider>();
            
            renderer.sharedMaterials = new[] { material };

            filter.transform.SetParent(rendererContainer);
            filter.gameObject.name = $"Track Mesh Renderer.{filter.transform.GetSiblingIndex()}";
            filter.transform.localPosition = Vector3.zero;
            filter.transform.localRotation = Quaternion.identity;

            var mesh = new Mesh();
            mesh.name = "[PROC] Track Segment";
            mesh.SetVertices(vertices.ToArray());
            mesh.SetNormals(normals.ToArray());
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            mesh.SetUVs(0, uvs.ToArray());
            if (optimize) mesh.Optimize();
            
            vertices.Clear();
            normals.Clear();
            indices.Clear();
            uvs.Clear();

            filter.sharedMesh = mesh;
            collider.sharedMesh = mesh;
        }

        private float FindNextPoint(TrackSegment segment, float startPoint, float segmentSize)
        {
            var start = segment.SamplePoint(startPoint);
            var step = 1.0f / (segment.Resolution * nextPointSubResolution);
            for (var p = startPoint + step; p <= 1.0f; p += step)
            {
                var end = segment.SamplePoint(p);

                var dist = (end - start).magnitude;
                if (dist > segmentSize) return p;
            }

            return 1.0f;
        }

#if UNITY_EDITOR
        [ContextMenu("Clear Track Mesh")]
        public void ClearBakedMesh()
        {
            var filter = GetComponent<MeshFilter>();
            if (filter && filter.sharedMesh)
            {
                DestroyImmediate(filter.sharedMesh);
                filter.sharedMesh = null;
            }

            var collider = GetComponent<MeshCollider>();
            if (collider && collider.sharedMesh)
            {
                DestroyImmediate(collider.sharedMesh);
                collider.sharedMesh = null;
            }
        }
#endif
    }
}