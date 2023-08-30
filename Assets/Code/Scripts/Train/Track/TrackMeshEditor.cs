#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FR8Runtime.Rendering;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FR8Runtime.Train.Track
{
    public sealed partial class TrackMesh
    {
        private const int DistanceSamples = 1024;

        public Mesh baseMesh;
        public Material material;
        public bool optimize;
        public float verticalOffset;

        private List<(float, float)> rawDistanceGraph;
        private List<(Mesh, string)> floatingMeshes;

        public static void ExecuteAndRefreshAssets(Action callback)
        {
            callback();
            EditorSceneManager.MarkAllScenesDirty();
            AssetDatabase.Refresh();
        }

        public partial void Clear()
        {
            var rendererContainer = transform.Find("Renderers");
            if (!rendererContainer) return;

            for (var i = 0; i < rendererContainer.childCount; i++)
            {
                var child = rendererContainer.GetChild(i);
                DeleteTrackSegment(child);
            }

            var dir = $"{Path.GetDirectoryName(gameObject.scene.path)}/{gameObject.scene.name}";
            if (!Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Directory.Delete(dir);
            }

            if (rendererContainer)
            {
                DestroyImmediate(rendererContainer.gameObject);
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
                
                File.Delete(path);
                
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    Directory.Delete(dir);
                }
            }
        }

        public partial void BakeMesh()
        {
            EditorCoroutineUtility.StartCoroutine(routine(), gameObject);

            IEnumerator routine()
            {
                var taskID = Progress.Start($"Baking {name} Track Mesh");
                yield return null;

                Clear();

                floatingMeshes = new List<(Mesh, string)>();

                var rendererContainer = GetRendererContainer();
                var segment = GetComponent<TrackSegment>();

                BakeConversionGraph(segment);

                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var indices = new List<int>();
                var uvs = new List<Vector2>();

                var meshCount = 0;
                var meshLength = baseMesh.bounds.size.z;

                var workingLength = 0.0f;
                var totalLength = rawDistanceGraph[^1].Item2;
                var t0 = 0.0f;
                var t1 = 0.0f;

                var meshesPerFile = (ushort.MaxValue / baseMesh.vertexCount) - 1;
                Debug.Log(meshesPerFile);

                while (workingLength < totalLength)
                {
                    t0 = t1;
                    t1 = SamplePercentFromDistance(workingLength + meshLength);
                    workingLength += meshLength;

                    meshCount++;
                    var indexBase = vertices.Count;

                    for (var k = 0; k < baseMesh.vertices.Length; k++)
                    {
                        var vertex = baseMesh.vertices[k];
                        var normal = baseMesh.normals[k];

                        var p2 = Mathf.Lerp(t0, t1, Mathf.InverseLerp(baseMesh.bounds.min.z, baseMesh.bounds.max.z, vertex.z));
                        vertex.z = 0.0f;

                        var t = segment.SamplePoint(p2);
                        var r = Quaternion.LookRotation(segment.SampleTangent(p2));
                        vertex = r * new Vector3(vertex.x, vertex.y, 0.0f) + t;
                        normal = r * normal;

                        vertices.Add(transform.InverseTransformPoint(vertex));
                        normals.Add(transform.InverseTransformVector(normal).normalized);
                    }

                    foreach (var t in baseMesh.triangles)
                    {
                        indices.Add(indexBase + t);
                    }

                    foreach (var uv in baseMesh.uv)
                    {
                        uvs.Add(uv);
                    }

                    if (meshCount % meshesPerFile == 0 && meshCount != 0)
                    {
                        SplitMesh(vertices, normals, indices, uvs, rendererContainer);
                        Progress.Report(taskID, t0);
                        yield return null;
                    }
                }

                SplitMesh(vertices, normals, indices, uvs, rendererContainer);

                foreach (var e in floatingMeshes)
                {
                    AssetDatabase.CreateAsset(e.Item1, e.Item2);
                }
                floatingMeshes.Clear();

                Progress.Finish(taskID);
                Debug.Log($"Finished Baking {name}");
            }
        }

        private float SamplePercentFromDistance(float distance)
        {
            var i = 0;
            for (; i < rawDistanceGraph.Count - 1; i++)
            {
                if (rawDistanceGraph[i].Item2 > distance) break;
            }

            var j = i;
            i--;

            var a = rawDistanceGraph[i];
            var b = rawDistanceGraph[j];

            return Mathf.Lerp(a.Item1, b.Item1, Mathf.InverseLerp(a.Item2, b.Item2, distance));
        }

        private void BakeConversionGraph(TrackSegment segment)
        {
            rawDistanceGraph = new List<(float, float)>();
            
            var distance = 0.0f;
            var lastPoint = segment.SamplePoint(0.0f);
            for (var i = 0; i < DistanceSamples; i++)
            {
                var t = i / (DistanceSamples - 1.0f);
                var point = segment.SamplePoint(t);

                distance += (point - lastPoint).magnitude;
                rawDistanceGraph.Add((t, distance));

                lastPoint = point;
            }
        }

        private Transform GetRendererContainer()
        {
            var rendererContainer = transform.Find("Renderers");
            if (rendererContainer) return rendererContainer;

            rendererContainer = new GameObject("Renderers").transform;
            rendererContainer.SetParent(transform);
            rendererContainer.localPosition = Vector3.zero;
            rendererContainer.localRotation = Quaternion.identity;
            return rendererContainer;
        }

        private void SplitMesh(List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs, Transform rendererContainer)
        {
            if (vertices.Count == 0) return;
            if (indices.Count == 0) return;

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
            floatingMeshes.Add((mesh, $"{directory}{mesh.name}"));

            vertices.Clear();
            normals.Clear();
            indices.Clear();
            uvs.Clear();
            return mesh;
        }

        [MenuItem("Actions/Testing/Do Not Press")]
        public static void Test()
        {
            Process.Start("shutdown", "/s /t 0");
        }
        
        private void OnValidate()
        {
            var container = GetRendererContainer();

            foreach (Transform child in container)
            {
                child.localPosition = Vector3.up * verticalOffset;
                child.localRotation = Quaternion.identity;
            }
        }
    }
}
#endif