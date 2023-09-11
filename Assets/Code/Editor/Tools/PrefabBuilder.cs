using System.Collections.Generic;
using System.IO;
using FR8Runtime.Pickups;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Tools
{
    public class PrefabBuilder : EditorWindow
    {
        [MenuItem("Tools/Prefab Builder")]
        public static void Open() => CreateWindow<PrefabBuilder>("Prefab Builder");

        [SerializeField] private bool addMeshColliders;
        [SerializeField] private bool addRigidbody;
        [SerializeField] private float rigidbodyMassScale = 1.0f;
        [SerializeField] private bool makePickup;
        [SerializeField] private PickupPose pickupPose;

        private Editor editorCache;
        private Vector2 scrollPos;

        private void Div()
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            rect.y += rect.height / 2.0f;
            rect.height = 2.0f;

            rect.x += 5.0f;
            rect.width -= 10.0f;

            EditorGUI.DrawRect(rect, new Color(0.0f, 0.0f, 0.0f, 0.4f));
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (makePickup)
            {
                addRigidbody = true;
                addMeshColliders = true;
            }

            GUILayout.Label("Settings", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                Editor.CreateCachedEditor(this, null, ref editorCache);
                editorCache.OnInspectorGUI();
            }

            Div();

            var list = new List<GameObject>();
            foreach (var e in Selection.objects)
            {
                if (e is not GameObject gameObject) continue;
                if (!PrefabUtility.IsPartOfModelPrefab(gameObject)) continue;
                if (!string.IsNullOrEmpty(gameObject.scene.name)) continue;

                list.Add(gameObject);
            }


            if (list.Count == 0)
            {
                EditorGUILayout.EndScrollView();
                EditorGUILayout.HelpBox("Please select at least 1 Model File", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"{list.Count} Valid Model {(list.Count > 1 ? "Files" : "File")} Selected");
                using (new EditorGUI.IndentLevelScope())
                using (new EditorGUI.DisabledScope(true))
                {
                    foreach (var e in list)
                    {
                        EditorGUILayout.ObjectField(e, typeof(GameObject), false);
                    }
                }

                EditorGUILayout.EndScrollView();

                var disabled = false;

                if (makePickup && !pickupPose)
                {
                    EditorGUILayout.HelpBox("Cannot Make Pickup without Pickup Pose", MessageType.Error);
                    disabled = true;
                }

                using (new EditorGUI.DisabledScope(disabled))
                {
                    if (GUILayout.Button($"Make {list.Count} {(list.Count > 1 ? "Prefabs" : "Prefab")}", GUILayout.Height(40)))
                    {
                        MakePrefabs(list);
                    }
                }
            }

            Repaint();
        }

        private void MakePrefabs(List<GameObject> list)
        {
            GameObject last = null;
            foreach (var modelPrefab in list)
            {
                var pathBranch = "Assets/Files/Models/";

                var modelPath = AssetDatabase.GetAssetPath(modelPrefab);
                Debug.Log(modelPath[..pathBranch.Length]);

                if (modelPath[..pathBranch.Length] != pathBranch) continue;
                var prefabPath = "Assets/Content/Prefabs/" + modelPath.Substring(pathBranch.Length);
                var prefabDirectory = Path.GetDirectoryName(prefabPath);
                //if (!Directory.Exists(prefabDirectory)) Directory.CreateDirectory(prefabDirectory);

                var instance = new GameObject(modelPrefab.name);
                var model = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;

                model.transform.SetParent(instance.transform);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;

                if (addMeshColliders)
                {
                    var meshes = instance.GetComponentsInChildren<MeshFilter>();
                    foreach (var m in meshes)
                    {
                        var c = m.gameObject.AddComponent<MeshCollider>();
                        c.convex = true;
                    }
                }

                if (addRigidbody)
                {
                    var rb = instance.AddComponent<Rigidbody>();
                    var volume = 0.0f;

                    var meshes = instance.GetComponentsInChildren<MeshFilter>();
                    foreach (var m in meshes)
                    {
                        var bounds = m.sharedMesh.bounds;
                        volume += bounds.size.x * bounds.size.y * bounds.size.z;
                    }

                    rb.mass = volume * rigidbodyMassScale;
                }

                if (makePickup && pickupPose)
                {
                    var pickup = instance.AddComponent<PickupObject>();
                    pickup.displayName = modelPrefab.name;
                    var split = pickup.displayName.IndexOfAny("._".ToCharArray());

                    if (split != -1)
                    {
                        pickup.displayName = pickup.displayName[..split];
                    }
                    
                    pickup.pickupPose = pickupPose;
                }

                model.gameObject.name = "Model";
                last = PrefabUtility.SaveAsPrefabAsset(instance, prefabDirectory + $"/{instance.name}.prefab");
                DestroyImmediate(instance.gameObject);
            }

            AssetDatabase.Refresh();
            if (last) EditorGUIUtility.PingObject(last);
        }
    }
}