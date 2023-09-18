#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FR8Runtime.Rendering;
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
        private static int trackMeshesBaking;

        public List<(float, float)> conversionGraph;

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

        public partial void BakeMesh()
        {
            var rendererContainer = GetRendererContainer(true);
            var segment = GetComponent<TrackSegment>();
            BakeConversionGraph(segment);

            bakeData = new TrackMeshBakeData(this, segment);
            bakeData.Cleanup();

            for (var i = 0; i < bakeData.meshes.Count; i++)
            {
                var meshData = bakeData.meshes[i];
                SplitMesh(i, meshData.vertices, meshData.normals, meshData.indices, meshData.uvs, rendererContainer);
            }

            bakeData = null;
            Debug.Log($"Finished Baking {name}");
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

        private Transform GetRendererContainer(bool createIfMissing)
        {
            var rendererContainer = transform.Find("Renderers");
            if (rendererContainer) return rendererContainer;

            if (!createIfMissing) return null;

            rendererContainer = new GameObject("Renderers").transform;
            rendererContainer.SetParent(transform);
            rendererContainer.localPosition = Vector3.zero;
            rendererContainer.localRotation = Quaternion.identity;
            return rendererContainer;
        }

        private void SplitMesh(int splitIndex, List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs, Transform rendererContainer)
        {
            if (vertices.Count == 0) return;
            if (indices.Count == 0) return;

            var container = splitIndex < rendererContainer.childCount ? rendererContainer.GetChild(splitIndex).gameObject : new GameObject();
            container.transform.SetParent(rendererContainer);
            container.transform.SetSiblingIndex(splitIndex);
            container.transform.localPosition = Vector3.up * verticalOffset;
            container.transform.localRotation = Quaternion.identity;

            var mesh = CompileMesh(container.transform, splitIndex, vertices, normals, indices, uvs);

            var filter = container.GetOrAddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            filter.gameObject.name = mesh.name;

            var renderer = container.GetOrAddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { material };

            var collider = container.GetOrAddComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            var marker = container.GetOrAddComponent<MapMarker>();
            marker.MarkerColor = Color.green;
        }

        private Mesh CompileMesh(Transform container, int splitIndex, List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> uvs)
        {
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);

            var offset = bounds.center;
            for (var i = 0; i < vertices.Count; i++) vertices[i] -= offset;
            container.position += offset;

            var mesh = GetMeshFromAssetDatabase(splitIndex, true);
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            if (optimize) mesh.Optimize();

            var directory = GetAssetPath(gameObject.name);
            if (!Directory.Exists($"./{directory}"))
            {
                Directory.CreateDirectory($"./{directory}");
            }

            return mesh;
        }

        private Mesh GetMeshFromAssetDatabase(int splitIndex, bool createIfMissing)
        {
            var name = GetSegmentName(splitIndex);
            var path = CreateAssetPath(WithFileExt(name));

            Mesh mesh;
            if (File.Exists(path))
            {
                mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                mesh.Clear();
                return mesh;
            }

            if (!createIfMissing) return null;

            mesh = new Mesh();
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private string GetSegmentName(int splitIndex) => $"{gameObject.scene.name.Replace(" ", "")}.{gameObject.name.Replace(" ", "")}.{splitIndex.ToString().PadLeft(3, '0')}";
        private string WithFileExt(string filename) => $"{filename}.asset";

        private string GetAssetPath(string subPath = "")
        {
            var hasSceneFolder = gameObject.scene.path.Contains($"\\{gameObject.scene.name}\\") || gameObject.scene.path.Contains($"/{gameObject.scene.name}/");

            return Path.Combine
            (
                Path.GetDirectoryName(gameObject.scene.path),
                hasSceneFolder ? string.Empty : gameObject.scene.name,
                "Track Bake",
                gameObject.name,
                subPath
            );
        }

        private string CreateAssetPath(string subPath = "")
        {
            var assetPath = GetAssetPath(subPath);
            var directory = Path.GetDirectoryName(assetPath);
            if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return assetPath;
        }

        private void OnValidate()
        {
            var container = GetRendererContainer(false);
            if (!container) return;

            var rng = new System.Random(gameObject.GetInstanceID());
            var verticalOffset = this.verticalOffset + (float)rng.NextDouble() * 0.01f;
            container.localPosition = Vector3.up * verticalOffset;
        }

        public void FindTrackMeshes()
        {
            var rendererContainer = GetRendererContainer(false);
            if (!rendererContainer) return;

            for (var i = 0; i < rendererContainer.childCount; i++)
            {
                var child = rendererContainer.GetChild(i);
                var mesh = GetMeshFromAssetDatabase(i, false);
                if (!mesh) continue;

                child.gameObject.name = mesh.name;

                if (child.TryGetComponent<MeshFilter>(out var filter))
                {
                    filter.sharedMesh = mesh;
                }

                if (child.TryGetComponent<MeshCollider>(out var collider))
                {
                    collider.sharedMesh = mesh;
                }
            }
        }

        public partial void Clear()
        {
            ExecuteAndRefreshAssets(() =>
            {
                var container = GetRendererContainer(false);
                if (container) DestroyImmediate(container.gameObject);

                var toDelete = new List<string>();
                foreach (var filename in Directory.EnumerateFiles(GetAssetPath(), "*.asset", SearchOption.AllDirectories))
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(filename) != typeof(Mesh)) continue;
                    toDelete.Add(filename);
                }

                AssetDatabase.DeleteAssets(toDelete.ToArray(), new List<string>());
            });
        }
    }
}
#endif