using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

        private Action finishCallback;

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

        public void GenerateAsync(TerrainData data, Action finishCallback)
        {
            if (generationThread != null && generationThread.IsAlive)
            {
                pendingChanges = true;
                return;
            }
         
            generationTimer.Reset();
            generationTimer.Start();

            if (finishCallback != null) this.finishCallback += finishCallback;
            
            pendingGenerator = GetGenerator(data);
            generationThread = pendingGenerator.GenerateAsync(-genSize * 0.5f, genSize * 0.5f, voxelSize);
        }

        private MarchingCubes GetGenerator(TerrainData data)
        {
            if (voxelSize < 0.0001f) return null;

            gameObject.GetOrAddCachedComponent(ref filter);
            if (!settings) settings = TerrainGenerationSettings.Fallback();

            return new MarchingCubes(data.AtPoint, 0.0001f, transform.position);
        }
        
        private void BuildMesh(MarchingCubes generator)
        {
            var mesh = generator.BuildMesh();
            EditorUtility.SetDirty(gameObject);

            filter.sharedMesh = mesh;
            if (gameObject.TryGetComponent(out MeshCollider collider)) collider.sharedMesh = mesh;
        }

        public void FinalizeGenerateAsync()
        {
            if (pendingGenerator == null) return;

            generationTimer.Stop();
            BuildMesh(pendingGenerator);
            
            pendingGenerator = null;
            finishCallback?.Invoke();
            finishCallback = null;

            if (!pendingChanges) return;
            
            var data = new TerrainData();
            GenerateAsync(data, finishCallback);
            pendingChanges = false;
        }
        
        public static void RegenerateAll()
        {
            var data = new TerrainData();
            foreach (var chunk in data.Chunks)
            {
                chunk.GenerateAsync(data, null);
            }
        }

        public static void RegenerateAll(Predicate<Chunk> predicate)
        {
            var data = new TerrainData();
            foreach (var chunk in data.Chunks)
            {
                if (!predicate(chunk)) continue;
                chunk.GenerateAsync(data, null);
            }
        }

        public static void RegenerateAllAsync()
        {
            var data = new TerrainData();
            var i = 0;

            recurse();
            
            void recurse()
            {
                if (i >= data.Chunks.Count) return;
                data.Chunks[i++].GenerateAsync(data, recurse);
            }
        }
    }
}