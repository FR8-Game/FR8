using TerrainHandles.Handles;
using UnityEditor;
using UnityEngine;

namespace TerrainHandles.Editor
{
    [InitializeOnLoad]
    public static class ChunkUpdatinator
    {
        private static float lastUpdateTime;
        private static bool finalUpdate;

        static ChunkUpdatinator()
        {
            ObjectChangeEvents.changesPublished += OnChangeEvent;
            EditorApplication.update += Update;
            Chunk.RegenerateAll();
        }

        private static void Update()
        {
            var chunks = Object.FindObjectsOfType<Chunk>();
            foreach (var chunk in chunks)
            {
                if (chunk.FinishedGeneration) chunk.FinalizeGenerateAsync();
            }
            
            if (finalUpdate) return;
            
            Chunk.RegenerateAll();
            finalUpdate = true;
        }

        private static void OnChangeEvent(ref ObjectChangeEventStream stream)
        {
            var projectSettings = ProjectSettings.Instance();
            if (projectSettings.updateFrequency > 0 && EditorApplication.timeSinceStartup - lastUpdateTime < 1.0f / projectSettings.updateFrequency) return;
            
            for (var i = 0; i < stream.length; i++)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.ChangeGameObjectOrComponentProperties) continue;
                stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var data);

                var instance = EditorUtility.InstanceIDToObject(data.instanceId);
                if (projectSettings.updateMethod == ProjectSettings.UpdateMethod.OnChangeEveryFrame) CallChangeFunction(instance);
                
                lastUpdateTime = (float)EditorApplication.timeSinceStartup;
                finalUpdate = false;
            }
        }

        private static void CallChangeFunction(Object instance, bool brokenDown = false)
        {
            while (true)
            {
                if (!instance) return;

                if (!brokenDown)
                {
                    if (instance is GameObject gameObject)
                    {
                        var components = gameObject.GetComponents<Component>();
                        foreach (var component in components)
                        {
                            CallChangeFunction(component, true);
                        }

                        return;
                    }
                }

                if (instance is not Component c) return;

                switch (instance)
                {
                    case TerrainHandle:
                    case Chunk:
                        Chunk.RegenerateAll();
                        return;
                    default:
                        instance = c.gameObject;
                        continue;
                }
            }
        }
    }
}