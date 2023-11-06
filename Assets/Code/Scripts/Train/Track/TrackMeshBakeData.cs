#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FR8Runtime.Train.Track
{
    public class TrackMeshBakeData
    {
        public List<MeshSegment> meshes = new();
        public List<Vector3> points;
        public List<Vector3> tangents;

        private MeshSegment baseMeshData;
        private float baseMeshZMin;
        private float baseMeshZMax;
        private Matrix4x4 matrix;

        private Thread thread;
        private int taskID;
        private string trackName;
        private Stopwatch threadTimer;

        public MeshSegment Segment => meshes[^1];
        public bool Done => true;

        public TrackMeshBakeData(TrackMesh trackMesh, Mesh baseMesh, TrackSegment segment)
        {
            trackName = trackMesh.name;

            baseMeshData = new MeshSegment();
            for (var i = 0; i < baseMesh.vertexCount; i++)
            {
                baseMeshData.vertices.Add(baseMesh.vertices[i]);
                baseMeshData.normals.Add(baseMesh.normals[i]);
                baseMeshData.uvs.Add(baseMesh.uv[i]);
            }

            foreach (var t in baseMesh.triangles)
            {
                baseMeshData.indices.Add(t);
            }

            baseMeshZMin = baseMesh.bounds.min.z;
            baseMeshZMax = baseMesh.bounds.max.z;

            matrix = trackMesh.transform.worldToLocalMatrix;
            Split();

            taskID = Progress.Start($"Baking {trackName} Track Mesh");

            threadTimer = new Stopwatch();
            threadTimer.Start();

            (points, tangents) = segment.GetBakeData();

            ThreadAction();
        }

        private void Split() => meshes.Add(new MeshSegment());

        // Runs on own thread
        private void ThreadAction()
        {   
            for (var i = 0; i < points.Count - 1; i++)
            {
                var p0 = points[i];
                var p1 = points[i + 1];

                var t0 = tangents[i];
                var t1 = tangents[i + 1];

                var r0 = Quaternion.LookRotation(t0);
                var r1 = Quaternion.LookRotation(t1);

                var indexBase = Segment.vertices.Count;
                for (var j = 0; j < baseMeshData.vertices.Count; j++)
                {
                    var vertex = baseMeshData.vertices[j];
                    var normal = baseMeshData.normals[j];

                    var z = Mathf.InverseLerp(baseMeshZMin, baseMeshZMax, vertex.z);
                    vertex.z = 0.0f;

                    var r = Quaternion.Slerp(r0, r1, z);
                    vertex = r * vertex + Vector3.Lerp(p0, p1, z);
                    normal = r * normal;

                    Segment.vertices.Add(matrix.MultiplyPoint(vertex));
                    Segment.normals.Add(matrix.MultiplyVector(normal).normalized);
                }
                
                foreach (var t in baseMeshData.indices)
                {
                    Segment.indices.Add(indexBase + t);
                }

                foreach (var uv in baseMeshData.uvs)
                {
                    Segment.uvs.Add(uv);
                }

                if (Segment.vertices.Count + baseMeshData.vertices.Count > ushort.MaxValue)
                {
                    Split();
                    Report(i / (float)points.Count);
                }
            }
        }

        public void Cleanup()
        {
            Progress.Finish(taskID);
            threadTimer.Stop();
            Debug.Log($"Finished Baking {trackName} in {threadTimer.ElapsedMilliseconds}");
        }

        private void Report(float progress)
        {
            Progress.Report(taskID, progress);
        }

        public class MeshSegment
        {
            public List<Vector3> vertices = new();
            public List<Vector3> normals = new();
            public List<int> indices = new();
            public List<Vector2> uvs = new();
        }
    }
}
#endif