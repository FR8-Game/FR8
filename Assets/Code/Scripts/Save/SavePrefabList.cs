using System.Collections.Generic;
using FR8Runtime.Train;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FR8Runtime.Save
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [CreateAssetMenu(menuName = "Scriptable Objects/Save System/Save Prefab List")]
    public class SavePrefabList : ScriptableObject
    {
        public TrainCarriage[] carriagePrefabs;

#if UNITY_EDITOR
        static SavePrefabList()
        {
            EditorApplication.projectChanged += RebuildPrefabList;
        }

        [ContextMenu("Get Prefabs From Assets")]
        public void GetPrefabsFromAssets()
        {
            var list = new List<TrainCarriage>();
            var guids = AssetDatabase.FindAssets("t:prefab");
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                var carriage = asset.GetComponent<TrainCarriage>();
                if (!carriage) continue;

                list.Add(carriage);
            }

            carriagePrefabs = list.ToArray();
            Debug.Log($"Rebuild Prefab Save List at \"{AssetDatabase.GetAssetPath(this)}\"");
        }

        [MenuItem("Actions/Save/Rebuild Save Prefab List")]
        public static void RebuildPrefabList()
        {
            var list = AssetDatabase.LoadAssetAtPath<SavePrefabList>("Assets/Config/Save/Save Prefab List.asset");
            list.GetPrefabsFromAssets();
        }
#endif
    }
}