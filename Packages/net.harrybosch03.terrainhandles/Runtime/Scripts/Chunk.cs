using System.Diagnostics;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace TerrainHandles
{
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private Vector3 genSize;
        [SerializeField] private float voxelSize;
        [SerializeField] private TerrainGenerationSettings settings;

        private MeshFilter filter;
        private Thread generationThread;
        private MarchingCubes pendingGenerator;
        private bool pendingChanges;

        public Vector3 GenSize => genSize;

        [ContextMenu("Generate")]
        public void Generate() => Generate(new TerrainData());

        public bool FinishedGeneration => generationThread != null && !generationThread.IsAlive && pendingGenerator != null;
        public bool PendingGeneration => generationThread != null && generationThread.IsAlive && pendingGenerator != null;
        public readonly Stopwatch generationTimer = new();
        
        public Bounds Bounds => new(transform.position, genSize);

        public void Generate(TerrainData data)
        {
            var generator = GetGenerator(data);
            generator.Generate(-genSize * 0.5f, genSize * 0.5f, voxelSize);
            BuildMesh(generator);
        }

        public void GenerateAsync(TerrainData data)
        {
            if (generationThread != null && generationThread.IsAlive)
            {
                pendingChanges = true;
                return;
            }
         
            generationTimer.Reset();
            generationTimer.Start();
            
            pendingGenerator = GetGenerator(data);
            generationThread = pendingGenerator.GenerateAsync(-genSize * 0.5f, genSize * 0.5f, voxelSize);
        }

        private MarchingCubes GetGenerator(TerrainData data)
        {
            if (voxelSize < 0.0001f) return null;

            gameObject.GetOrAddCachedComponent(ref filter);
            if (!settings) settings = TerrainGenerationSettings.Fallback();

            return new MarchingCubes(data.AtPoint, settings.threshold, transform.position);
        }
        
        private void BuildMesh(MarchingCubes generator)
        {
            var mesh = generator.BuildMesh();
            EditorUtility.SetDirty(gameObject);

            filter.sharedMesh = mesh;
        }

        public void FinalizeGenerateAsync()
        {
            if (pendingGenerator == null) return;

            generationTimer.Stop();
            BuildMesh(pendingGenerator);
            
            pendingGenerator = null;

            if (pendingChanges)
            {
                var data = new TerrainData();
                GenerateAsync(data);
                pendingChanges = false;
            }
        }
        
        public static void RegenerateAll()
        {
            var data = new TerrainData();
            foreach (var chunk in data.Chunks)
            {
                chunk.GenerateAsync(data);
            }
        }
    }
}