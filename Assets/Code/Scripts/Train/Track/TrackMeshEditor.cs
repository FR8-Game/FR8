#if UNITY_EDITOR

using System;
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
    public sealed partial class TrackMesh
    {
        private const int DistanceSamples = 1024;

        public Mesh baseMesh;
        public Material material;
        public bool optimize;
        public float verticalOffset;

        private TrackMeshBakeData bakeData;

        public List<(float, float)> conversionGraph;
        private List<(Mesh, string)> floatingMeshes;

        public static void ExecuteAndRefreshAssets(Action callback)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                callback();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorSceneManager.MarkAllScenesDirty();
                AssetDatabase.Refresh();
            }
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
            if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
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
                yield return null;

                Clear();

                floatingMeshes = new List<(Mesh, string)>();

                var rendererContainer = GetRendererContainer();
                var segment = GetComponent<TrackSegment>();
                BakeConversionGraph(segment);

                bakeData = new TrackMeshBakeData(this, segment);

                yield return new WaitUntil(() => bakeData.Done);
                bakeData.Cleanup();

                foreach (var meshData in bakeData.meshes)
                {
                    SplitMesh(meshData.vertices, meshData.normals, meshData.indices, meshData.uvs, rendererContainer);
                }

                foreach (var e in floatingMeshes)
                {
                    AssetDatabase.CreateAsset(e.Item1, e.Item2);
                }

                floatingMeshes.Clear();

                bakeData = null;
                Debug.Log($"Finished Baking {name}");
            }
        }

        private void BakeConversionGraph(TrackSegment segment)
        {
            conversionGraph = new List<(float, float)>();

            var distance = 0.0f;
            var lastPoint = segment.SamplePoint(0.0f);
            for (var i = 0; i < DistanceSamples; i++)
            {
                var t = i / (DistanceSamples - 1.0f);
                var point = segment.SamplePoint(t);

                distance += (point - lastPoint).magnitude;
                conversionGraph.Add((t, distance));

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
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            if (optimize) mesh.Optimize();

            var directory = $"{Path.GetDirectoryName(gameObject.scene.path)}/{gameObject.scene.name}/Track Bake/{gameObject.name}/";
            if (!Directory.Exists($"./{directory}"))
            {
                Directory.CreateDirectory($"./{directory}");
            }

            mesh.name = $"Track Mesh.{(uint)mesh.GetHashCode()}.asset";
            floatingMeshes.Add((mesh, $"{directory}{mesh.name}"));

            return mesh;
        }

        private void OnValidate()
        {
            var container = GetRendererContainer();

            var rng = new System.Random(gameObject.GetInstanceID());
            var verticalOffset = this.verticalOffset + (float)rng.NextDouble() * 0.01f;
            
            foreach (Transform child in container)
            {
                child.localPosition = Vector3.up * verticalOffset;
                child.localRotation = Quaternion.identity;
            }
        }
    }
}
#endif