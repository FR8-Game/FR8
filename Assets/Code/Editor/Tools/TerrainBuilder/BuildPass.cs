
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Editor.Tools.TerrainBuilder
{
    [System.Serializable]
    public class BuildPass
    {
        #region Properties

        #endregion

        // UITK References
        private ObjectField textField;
        private ObjectField matField;
        private Vector2Field mapSizeField;
        private FloatField vertScaleField;
        private Button bakeButton;
        private HelpBox info;
        private DropdownField resDropdown;

        // Caches
        private Transform parent;
        private Terrain[,] terrainElements;

        // Calculated Values
        private Texture2D inputTex;
        private Material material;
        private int terrainResolution;
        private Vector2 mapSize;
        private Vector2 terrainSize;

        // Other Values
        private string dataPath;
        private int xTile, yTile;

        // Constants
        private const string ParentName = "TerrainBuilder.BuildResults";

        public void CreateGUI(VisualElement root)
        {
            root.style.paddingBottom = 20;
            root.style.paddingLeft = 20;
            root.style.paddingRight = 20;
            root.style.paddingTop = 20;
            
            var resOptions = new List<string>();
            for (var i = 5; i < 13; i++)
            {
                var r = (0b1 << i) + 1;
                resOptions.Add($"{r} x {r}");
            }

            root.Add(textField = new ObjectField("Input Texture") { objectType = typeof(Texture2D) });
            root.Add(matField = new ObjectField("Terrain Material") { objectType = typeof(Material) });
            root.Add(resDropdown = new DropdownField("Sub Texture Resolution", resOptions, 4));
            root.Add(mapSizeField = new Vector2Field("Map Size [KM]"));
            root.Add(vertScaleField = new FloatField("Vertical Scale [M]"));

            UpdateValuesFromEditor();
            info = new HelpBox();
            root.Add(info);
            UpdateBuildInfo();

            bakeButton = new Button(Bake) { text = "Build Terrain" };
            bakeButton.style.marginTop = 10;
            bakeButton.style.height = 30;
            root.Add(bakeButton);
        }

        public void Update()
        {
            if (bakeButton != null) bakeButton.SetEnabled(Valid());
            UpdateBuildInfo();
        }

        private void UpdateBuildInfo()
        {
            if (info == null) return;

            UpdateValuesFromEditor();

            info.text = $"Generation Information\n\tTerrain Resolution: {terrainResolution} x {terrainResolution}";
            info.messageType = Valid() ? HelpBoxMessageType.Info : HelpBoxMessageType.Error;
        }

        private bool Valid()
        {
            if (!textField.value) return false;
            if (terrainResolution < 33) return false;
            if (terrainResolution > 8193) return false;

            return true;
        }

        private void UpdateValuesFromEditor()
        {
            terrainResolution = (0b1 << (resDropdown.index + 5)) + 1;
            mapSize = mapSizeField.value * 1000.0f;
        }

        private void Bake()
        {
            if (!EditorUtility.DisplayDialog("Bake Terrain Textures", "This Action make Take a While.", "Continue", "Cancel")) return;

            AssetDatabase.StartAssetEditing();
            try
            {
                UpdateValuesFromEditor();

                var find = GameObject.Find(ParentName);
                if (find) Object.DestroyImmediate(find);

                parent = new GameObject(ParentName).transform;
                parent.transform.position = Vector3.zero;
                parent.transform.rotation = Quaternion.identity;

                inputTex = (Texture2D)textField.value;
                material = (Material)matField.value;
                if (!material) material = (Material)EditorUtility.InstanceIDToObject(24472);
                
                xTile = Mathf.CeilToInt((float)inputTex.width / terrainResolution);
                yTile = Mathf.CeilToInt((float)inputTex.height / terrainResolution);
                var textureCount = xTile * yTile;
                terrainElements = new Terrain[xTile, yTile];

                terrainSize = mapSize / new Vector2(xTile, yTile);

                dataPath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(inputTex)), "Bake Results");
                Directory.CreateDirectory(dataPath);

                foreach (var file in Directory.EnumerateFiles(dataPath, "*.asset", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }

                for (var i = 0; i < textureCount; i++)
                {
                    var x0 = i % xTile;
                    var y0 = i / xTile;

                    var u0 = x0 / (float)xTile;
                    var v0 = y0 / (float)yTile;

                    var terrain = GetTerrain(x0, y0, u0, v0);
                    terrainElements[x0, y0] = terrain;
                }

                EditorSceneManager.MarkAllScenesDirty();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private Terrain GetTerrain(int x0, int y0, float u0, float v0)
        {
            var terrain = new GameObject("Terrain").AddComponent<Terrain>();
            var collider = terrain.gameObject.AddComponent<TerrainCollider>();
            terrain.gameObject.name = $"[PROC] Terrain.[{x0}, {y0}]";
            terrain.transform.SetParent(parent);
            
            terrain.materialTemplate = material;

            var terrainData = new TerrainData();
            terrainData.name = $"{terrain.name}.data";
            terrain.terrainData = terrainData;
            collider.terrainData = terrainData;

            terrain.transform.position = new Vector3(x0 * terrainSize.x, 0.0f, y0 * terrainSize.y) - new Vector3(mapSize.x, 0.0f, mapSize.y) / 2.0f;
            var heights = new float[terrainResolution, terrainResolution];
            for (var x1 = 0; x1 < terrainResolution; x1++)
            for (var y1 = 0; y1 < terrainResolution; y1++)
            {
                var u1 = (x1 / (terrainResolution - 1.0f)) / xTile + u0;
                var v1 = (y1 / (terrainResolution - 1.0f)) / yTile + v0;

                heights[y1, x1] = inputTex.GetPixelBilinear(u1, v1).r;
            }

            terrainData.heightmapResolution = terrainResolution;
            terrainData.SetHeights(0, 0, heights);
            terrainData.size = new Vector3(terrainSize.x, vertScaleField.value, terrainSize.y);

            AssetDatabase.CreateAsset(terrainData, Path.Combine(dataPath, $"{terrainData.name}.asset"));

            return terrain;
        }
    }
}