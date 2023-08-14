﻿#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FR8.Train.Track
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
                string dir;
                for (var i = 0; i < rendererContainer.childCount; i++)
                {
                    Progress.Report(taskID, i / (float)rendererContainer.childCount);
                    
                    var child = rendererContainer.GetChild(i);
                    
                    var filter = child.GetComponent<MeshFilter>();
                    var mesh = filter.sharedMesh;
                    if (mesh && AssetDatabase.IsMainAsset(mesh))
                    {
                        var path = AssetDatabase.GetAssetPath(mesh);
                        dir = Path.GetDirectoryName(path);
                        AssetDatabase.DeleteAsset(path);
                        if (!Directory.EnumerateFileSystemEntries(dir).Any())
                        {
                            AssetDatabase.DeleteAsset(dir);
                        }
                    }

                    yield return null;
                }

                dir = $"{Path.GetDirectoryName(gameObject.scene.path)}/{gameObject.scene.name}";
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
                AssetDatabase.Refresh();
                
                Progress.Finish(taskID);
            }
        }

        public partial void BakeMesh()
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
                AssetDatabase.Refresh();
            }
        }

        private void SplitMesh(List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs, Transform rendererContainer)
        {
            var filter = new GameObject().AddComponent<MeshFilter>();
            var renderer = filter.gameObject.AddComponent<MeshRenderer>();
            var collider = filter.gameObject.AddComponent<MeshCollider>();

            renderer.sharedMaterials = new[] { material };

            filter.transform.SetParent(rendererContainer);
            filter.transform.SetAsLastSibling();

            var index = filter.transform.GetSiblingIndex();

            filter.gameObject.name = $"Track Mesh Renderer.{index}";
            filter.transform.localPosition = Vector3.up * verticalOffset;
            filter.transform.localRotation = Quaternion.identity;

            var mesh = new Mesh();
            mesh.name = $"[PROC] {name}.Mesh.{index}";
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
            filter.name = mesh.name;
            AssetDatabase.CreateAsset(mesh, $"{directory}{mesh.name}");

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