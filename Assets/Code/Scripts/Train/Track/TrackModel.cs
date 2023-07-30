using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FR8.Train.Track
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrackSegment), typeof(MeshFilter))]
    public sealed class TrackModel : MonoBehaviour
    {
        private const int maxSegments = 1000;

        [SerializeField] private Mesh baseMesh;

        private void Awake()
        {
            BakeMesh();
        }

        [ContextMenu("Bake Track Mesh")]
        public void BakeMesh()
        {
            var segment = GetComponent<TrackSegment>();
            var filter = GetComponent<MeshFilter>();

            var mesh = new Mesh();
            mesh.name = "[PROC] Track Segment";

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var indices = new List<int>();
            var uvs = new List<Vector2>();

            var count = GetCount(segment);

            for (var i = 0; i < count; i++)
            {
                var p0 = i / (float)count;
                var p1 = (i + 1.0f) / count;

                var indexBase = vertices.Count;

                foreach (var v0 in baseMesh.vertices)
                {
                    var p2 = Mathf.Lerp(p0, p1, Mathf.InverseLerp(baseMesh.bounds.min.z, baseMesh.bounds.max.z, v0.z));

                    var t = segment.SamplePoint(p2);
                    var r = Quaternion.LookRotation(segment.SampleTangent(p2));
                    var v2 = r * new Vector3(v0.x, v0.y, 0.0f) + t;

                    vertices.Add(transform.InverseTransformPoint(v2));
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
            }

            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices.ToArray());
            mesh.SetNormals(normals.ToArray());
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            mesh.SetUVs(0, uvs.ToArray());
            mesh.Optimize();

            filter.sharedMesh = mesh;

            var collider = GetComponent<MeshCollider>();
            if (collider)
            {
                collider.sharedMesh = mesh;
            }
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

        private int GetCount(TrackSegment segment)
        {
            var size = baseMesh.bounds.size.z;

            for (var i = 2; i < maxSegments; i++)
            {
                var dist = getSubArcLength(1.0f / i);
                if (size > dist) return i;
            }

            throw new Exception("Track is too long, it exceeds the maximum amount of segments.");

            float getSubArcLength(float subArcLength)
            {
                const int resolution = 20;
                
                var distance = 0.0f;
                for (var i = 0; i < resolution; i++)
                {
                    var p0 = (i / (float)resolution) * subArcLength;
                    var p1 = ((i + 1.0f) / resolution ) * subArcLength;

                    distance += (segment.SamplePoint(p1) - segment.SamplePoint(p0)).magnitude;
                }
                return distance;
            }
        }
    }
}