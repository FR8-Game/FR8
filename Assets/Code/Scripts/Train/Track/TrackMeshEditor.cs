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
        private const int RasterizeResolution = 2;

        public Mesh baseMesh;
        public Material material;
        public bool optimize;
        public int segmentsPerSplit = 15;
        public float verticalOffset;

        private List<Vector3> trackPoints;
        private List<Vector3> trackVelocities;

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
                AssetDatabase.DeleteAsset(dir);
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
            var toDelete = new List<string>();

            if (mesh && AssetDatabase.IsMainAsset(mesh))
            {
                var path = AssetDatabase.GetAssetPath(mesh);
                var dir = Path.GetDirectoryName(path);
                toDelete.Add(path);
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    toDelete.Add(dir);
                }
            }

            AssetDatabase.DeleteAssets(toDelete.ToArray(), new List<string>());
        }

        public partial void BakeMesh()
        {
            Clear();

            var rendererContainer = GetRendererContainer();
            var segment = GetComponent<TrackSegment>();

            RasterizeTrack(segment);

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var indices = new List<int>();
            var uvs = new List<Vector2>();

            var startPoint = 0.0f;
            var endPoint = 0.0f;
            var segmentSize = baseMesh.bounds.size.z;

            var index = 0;

            var taskID = Progress.Start($"[{name}] Baking Track Mesh");

            while (endPoint < 1.0f)
            {
                if (index != 0 && index % segmentsPerSplit == 0)
                {
                    Progress.Report(taskID, endPoint);
                    SplitMesh(vertices, normals, indices, uvs, rendererContainer);
                }

                endPoint = FindNextPoint(startPoint, segmentSize);

                var indexBase = vertices.Count;

                for (var i = 0; i < baseMesh.vertices.Length; i++)
                {
                    var vertex = baseMesh.vertices[i];
                    var normal = baseMesh.normals[i];

                    var p2 = Mathf.Lerp(startPoint, endPoint, Mathf.InverseLerp(baseMesh.bounds.min.z, baseMesh.bounds.max.z, vertex.z));
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

                startPoint = endPoint;
                index++;
            }

            SplitMesh(vertices, normals, indices, uvs, rendererContainer);

            Progress.Remove(taskID);
            Debug.Log($"Finished Baking {name}");
        }

        private void RasterizeTrack(TrackSegment segment)
        {
            trackPoints = new List<Vector3>();
            trackVelocities = new List<Vector3>();

            var length = 0.0f;
            for (var i = 0; i < segment.Count - 1; i++)
            {
                var a = segment[i].position;
                var b = segment[i + 1].position;

                length += (b - a).magnitude;
            }

            var resolution = Mathf.Ceil(length * RasterizeResolution);
            for (var i = 0; i < resolution; i++)
            {
                var p = (i - 1.0f) / resolution;
                trackPoints.Add(segment.SamplePoint(p));
                trackVelocities.Add(segment.SampleVelocity(p));
            }
            
            Debug.Log($"Track Rasterized to {resolution} points");
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

        private float FindNextPoint(float startPoint, float segmentLength)
        {
            var i = Mathf.FloorToInt(startPoint * trackPoints.Count);

            var length = 0.0f;

            for (var j = i + 1; j < trackPoints.Count; j++)
            {
                var a = trackPoints[j - 1];
                var b = trackPoints[j];
                length += (b - a).magnitude;

                if (length > segmentLength) return j / (float)trackPoints.Count;
            }

            for (var k = 2; k < 10000; k++)
            {
                var a = trackPoints[^2];
                var b = trackPoints[^1];

                var v = k * (b - a);
                if (v.magnitude > segmentLength) return (trackPoints.Count - 2.0f + k) / trackPoints.Count;
            }
            throw new Exception();
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