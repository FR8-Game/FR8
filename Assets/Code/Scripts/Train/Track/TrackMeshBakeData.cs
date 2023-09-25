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

        private List<(float, float)> conversionGraph;
        private MeshSegment baseMeshData;
        private float baseMeshLength;
        private float baseMeshZMin;
        private float baseMeshZMax;
        private Matrix4x4 matrix;
        private TrackSegment segment;

        private Thread thread;
        private int taskID;
        private string trackName;
        private Stopwatch threadTimer;

        public MeshSegment Segment => meshes[^1];
        public bool Done => true;

        public TrackMeshBakeData(TrackMesh trackMesh, Mesh baseMesh, TrackSegment segment)
        {
            trackName = trackMesh.name;
            conversionGraph = trackMesh.conversionGraph;
            this.segment = segment;
            
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

            baseMeshLength = baseMesh.bounds.size.z;
            baseMeshZMin = baseMesh.bounds.min.z;
            baseMeshZMax = baseMesh.bounds.max.z;

            matrix = trackMesh.transform.worldToLocalMatrix;
            Split();

            taskID = Progress.Start($"Baking {trackName} Track Mesh");

            threadTimer = new Stopwatch();
            threadTimer.Start();

            ThreadAction();
        }

        private void Split() => meshes.Add(new MeshSegment());

        // Runs on own thread
        private void ThreadAction()
        {
            var meshCount = 0;
            var workingLength = 0.0f;
            var totalLength = conversionGraph[^1].Item2;
            var t0 = 0.0f;
            var t1 = 0.0f;

            var meshesPerFile = (ushort.MaxValue / baseMeshData.vertices.Count) - 1;

            while (workingLength < totalLength)
            {
                t0 = t1;
                t1 = samplePercentFromDistance(workingLength + baseMeshLength);
                workingLength += baseMeshLength;

                meshCount++;
                var indexBase = Segment.vertices.Count;

                for (var k = 0; k < baseMeshData.vertices.Count; k++)
                {
                    var vertex = baseMeshData.vertices[k];
                    var normal = baseMeshData.normals[k];

                    var p2 = Mathf.Lerp(t0, t1, Mathf.InverseLerp(baseMeshZMin, baseMeshZMax, vertex.z));
                    vertex.z = 0.0f;

                    //var splinePoint = TrackSegment.Sample(p2, (spline, t) => spline.EvaluatePoint(t), i => spline[i], spline.Count);
                    var splinePoint = segment.SamplePoint(p2);
                    var splineTangent = segment.SampleTangent(p2);
                    var r = Quaternion.LookRotation(splineTangent);

                    vertex = r * new Vector3(vertex.x, vertex.y, 0.0f) + splinePoint;
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

                if (meshCount % meshesPerFile == 0 && meshCount != 0)
                {
                    Split();
                    Report(workingLength / totalLength);
                }
            }

            float samplePercentFromDistance(float distance)
            {
                var i = 0;
                for (; i < conversionGraph.Count - 1; i++)
                {
                    if (conversionGraph[i].Item2 > distance) break;
                }

                var j = i;
                i--;

                var a = conversionGraph[i];
                var b = conversionGraph[j];

                return Mathf.Lerp(a.Item1, b.Item1, Mathf.InverseLerp(a.Item2, b.Item2, distance));
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