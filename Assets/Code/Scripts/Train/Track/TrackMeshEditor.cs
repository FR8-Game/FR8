#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FR8Runtime.Rendering;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FR8Runtime.Train.Track
{
    [InitializeOnLoad]
    public sealed partial class TrackMesh
    {
        private const int NextPointSubResolution = 10;

        [SerializeField] private Mesh baseMesh;
        [SerializeField] private Material material;
        [SerializeField] private bool optimize;
        [SerializeField] private int segmentsPerSplit = 15;
        [SerializeField] private float verticalOffset;

        public partial void Clear()
        {
            var rendererContainer = transform.Find("Renderers");
            if (!rendererContainer) return;

            var taskID = Progress.Start($"Clearing {name}");
            EditorCoroutineUtility.StartCoroutine(routine(), gameObject);
            
            IEnumerator routine()
            {
                for (var i = 0; i < rendererContainer.childCount; i++)
                {
                    Progress.Report(taskID, i / (float)rendererContainer.childCount);
                    
                    var child = rendererContainer.GetChild(i);
                    DeleteTrackSegment(child);

                    yield return null;
                }

                var dir = $"{Path.GetDirectoryName(gameObject.scene.path)}/{gameObject.scene.name}";
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    AssetDatabase.DeleteAsset(dir);
                }

                if (rendererContainer)
                {
                    DestroyImmediate(rendererContainer.gameObject);
                }

                EditorSceneManager.MarkSceneDirty(gameObject.scene);
                AssetDatabase.SaveAssets();
                
                Progress.Finish(taskID);
            }
        }

        private static void DeleteTrackSegment(Transform child)
        {
            var filter = child.GetComponent<MeshFilter>();
            var mesh = filter.sharedMesh;
            if (mesh && AssetDatabase.IsMainAsset(mesh))
            {
                var path = AssetDatabase.GetAssetPath(mesh);
                var dir = Path.GetDirectoryName(path);
                AssetDatabase.DeleteAsset(path);
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    AssetDatabase.DeleteAsset(dir);
                }
            }
        }

        public partial void BakeMesh()
        {
            Clear();

            var rendererContainer = GetRendererContainer();
            var segment = GetComponent<TrackSegment>();

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var indices = new List<int>();
            var uvs = new List<Vector2>();

            var startPoint = 0.0f;
            var endPoint = 0.0f;
            var segmentSize = baseMesh.bounds.size.z;

            var index = 0;
            EditorCoroutineUtility.StartCoroutine(progress(), gameObject);

            IEnumerator progress()
            {
                var taskID = Progress.Start($"[{name}] Baking Track Mesh");

                while (endPoint < 1.0f)
                {
                    if (index != 0 && index % segmentsPerSplit == 0)
                    {
                        Progress.Report(taskID, endPoint);
                        SplitMesh(vertices, normals, indices, uvs, rendererContainer);
                        yield return null;
                    }

                    endPoint = FindNextPoint(segment, startPoint, segmentSize);

                    var indexBase = vertices.Count;

                    foreach (var v0 in baseMesh.vertices)
                    {
                        var p2 = Mathf.Lerp(startPoint, endPoint, Mathf.InverseLerp(baseMesh.bounds.min.z, baseMesh.bounds.max.z, v0.z));

                        var t = segment.SamplePoint(p2);
                        var r = Quaternion.LookRotation(segment.SampleTangent(p2));
                        var v2 = r * new Vector3(v0.x, v0.y, 0.0f) + t;

                        vertices.Add(transform.InverseTransformPoint(v2));
                    }

                    foreach (var n in baseMesh.normals)
                    {
                        normals.Add(transform.InverseTransformDirection(n).normalized);
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

                Progress.Remove(taskID);
                Debug.Log($"Finished Baking {name}");
                EditorSceneManager.MarkSceneDirty(gameObject.scene);

                AssetDatabase.SaveAssets();
            }
        }

        private Transform GetRendererContainer()
        {
            var rendererContainer = new GameObject("Renderers").transform;
            rendererContainer.SetParent(transform);
            rendererContainer.localPosition = Vector3.zero;
            rendererContainer.localRotation = Quaternion.identity;
            return rendererContainer;
        }

        private void SplitMesh(List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs, Transform rendererContainer)
        {
            var mesh = CompileMesh(vertices, normals, indices, uvs);
            
            var filter = new GameObject().AddComponent<MeshFilter>();
            filter.transform.SetParent(rendererContainer);
            filter.transform.SetAsLastSibling();
            filter.sharedMesh = mesh;
            filter.gameObject.name = mesh.name;
            filter.transform.localPosition = Vector3.up * verticalOffset;
            filter.transform.localRotation = Quaternion.identity;
            
            var renderer = filter.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { material };
            
            var collider = filter.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            
            var marker = filter.gameObject.AddComponent<MapMarker>();
            marker.MarkerColor = Color.green;
        }

        private Mesh CompileMesh(List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertices.ToArray());
            mesh.SetNormals(normals.ToArray());
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            mesh.SetUVs(0, uvs.ToArray());
            if (optimize) mesh.Optimize();

            var directory = $"{Path.GetDirectoryName(gameObject.scene.path)}/{gameObject.scene.name}/{gameObject.name}/";
            if (!Directory.Exists($"./{directory}"))
            {
                Directory.CreateDirectory($"./{directory}");
            }

            mesh.name = $"Track Mesh.{(uint)mesh.GetHashCode()}.asset";
            AssetDatabase.CreateAsset(mesh, $"{directory}{mesh.name}");

            vertices.Clear();
            normals.Clear();
            indices.Clear();
            uvs.Clear();
            return mesh;
        }

        private float FindNextPoint(TrackSegment segment, float startPoint, float segmentSize)
        {
            var start = segment.SamplePoint(startPoint);
            var step = 1.0f / (segment.Resolution * NextPointSubResolution);
            for (var p = startPoint + step; p <= 1.0f; p += step)
            {
                var end = segment.SamplePoint(p);

                var dist = (end - start).magnitude;
                if (dist > segmentSize) return p;
            }

            return 1.0f;
        }

        private void OnValidate()
        {
            if (transform.childCount == 0) return;

            foreach (Transform child in transform.GetChild(0))
            {
                child.localPosition = Vector3.up * verticalOffset;
                child.localRotation = Quaternion.identity;
            }
        }
    }
}
#endif