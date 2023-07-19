using UnityEngine;

namespace TerrainHandles
{
    [CreateAssetMenu(menuName = "Terrain Handles/Terrain Generation Settings")]
    public class TerrainGenerationSettings : ScriptableObject
    {
        public float threshold = 0.5f;

        public static TerrainGenerationSettings Fallback() => ProjectSettings.Instance().FallbackSettings;
    }
}
