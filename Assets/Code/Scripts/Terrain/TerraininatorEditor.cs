#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FR8Runtime.Terrain
{
    public partial class Terraininator
    {
        [SerializeField] private Texture2D basemap;
        [SerializeField] private Vector2 terrainSize = Vector2.one * 18000.0f;
        [SerializeField] private float terrainHeight = 100.0f;
        [SerializeField][Range(5, 12)] private int resolution;

        private int tileX, tileY, terrainCount;
        private List<TerrainData> dataList;
        
        public int HeightmapResolution => (0b1 << resolution) + 1;
        public string DataPath => Path.Combine(Application.dataPath, $"Files/Terrain/Generated/{gameObject.scene.name}.{gameObject.name}");

        public void Refresh()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                tileX = Mathf.CeilToInt((float)basemap.width / HeightmapResolution);
                tileY = Mathf.CeilToInt((float)basemap.height / HeightmapResolution);
                terrainCount = tileX * tileY;

                GetTerrainData();

                SpawnTerrainInScene();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private void GetTerrainData()
        {
            dataList = new List<TerrainData>();

            if (Directory.Exists(DataPath))
            {
                foreach (var path in Directory.EnumerateDirectories(DataPath, "*.asset", SearchOption.AllDirectories))
                {
                    dataList.Add(AssetDatabase.LoadAssetAtPath<TerrainData>(path));
                }
            }
            else Directory.CreateDirectory(DataPath);

            while (dataList.Count > terrainCount)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(dataList[^1]));
                dataList.RemoveAt(dataList.Count - 1);
            }

            while (dataList.Count < terrainCount)
            {
                var newData = new TerrainData();
                newData.name = $"{gameObject.scene.name}.{gameObject.name}.{newData.GetInstanceID()}";
                AssetDatabase.CreateAsset(newData, Path.Combine(DataPath, $"{newData.name}.asset"));
                dataList.Add(newData);
            }
        }
        
        private void SpawnTerrainInScene()
        {
            var terrainList = new List<UnityEngine.Terrain>(GetComponentsInChildren<UnityEngine.Terrain>());
            while (terrainList.Count < terrainCount)
            {
                var instance = new GameObject("terrain").AddComponent<UnityEngine.Terrain>();
                instance.transform.SetParent(transform);
                instance.gameObject.AddComponent<TerrainCollider>();
            }

            while (terrainList.Count > terrainCount)
            {
                var delete = terrainList[^1];
                terrainList.RemoveAt(terrainList.Count - 1);

                DestroyImmediate(delete);
            }

            for (var i = 0; i < terrainCount; i++)
            {
                var x = i / tileX;
                var y = i % tileX;

                var terrain = terrainList[i];
                terrain.terrainData = dataList[i];
                terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;
                terrain.gameObject.name = terrain.terrainData.name;
            }
        }
    }

}

#endif