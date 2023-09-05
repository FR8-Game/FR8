using System.Collections.Generic;
using FR8Runtime.Player;
using FR8Runtime.Train;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FR8Runtime.Save
{
    [SelectionBase, DisallowMultipleComponent]
    public class SceneSaveManager : MonoBehaviour
    {
        [SerializeField] private SavePrefabList prefabList;
        
        private void Awake()
        {
            if (!SaveManager.SlotSave.HasSave()) return;

            var data = SaveManager.SlotSave.GetOrLoad();

            if (data.trains != null)
            {
                var trains = FindObjectsOfType<TrainCarriage>();

                foreach (var t in trains)
                {
                    Destroy(t.gameObject);
                }
                
                var trainTable = new Dictionary<string, TrainCarriage>();
                foreach (var prefab in prefabList.carriagePrefabs)
                {
                    trainTable.Add(prefab.saveTypeReference, prefab);
                }
                
                foreach (var entry in data.trains)
                {
                    var train = Instantiate(trainTable[entry.saveTypeReference]);
                    entry.Apply(train);
                }
            }

            if (data.player != null)
            {
                var player = FindObjectOfType<PlayerAvatar>();
                data.player.Apply(player);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Actions/Save Game")]
        public static void SaveEditor()
        {
            var manager = FindObjectOfType<SceneSaveManager>();
            if (!manager) return;
            manager.Save();
        }

        private void Reset()
        {
            prefabList = AssetDatabase.LoadAssetAtPath<SavePrefabList>("Assets/Config/Save/Save Prefab List.asset");
        }
#endif

        public void Save()
        {
            var data = SaveManager.SlotSave.GetOrLoad();

            var trains = FindObjectsOfType<TrainCarriage>();
            data.trains = new TrainSaveData[trains.Length];
            for (var i = 0; i < trains.Length; i++)
            {
                data.trains[i] = new TrainSaveData(trains[i]);
            }

            var player = FindObjectOfType<PlayerAvatar>();
            data.player = new PlayerSaveData(player);

            SaveManager.SlotSave.Save();
        }
    }
}