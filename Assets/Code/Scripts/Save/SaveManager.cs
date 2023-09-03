using UnityEngine;

namespace FR8Runtime.Save
{
    public static class SaveManager
    {
        public static string currentSaveName = "New Save";
        
        public static readonly SaveHelper<SaveData> SlotSave = new(() => $"{Application.dataPath}/Saves/{currentSaveName}.sav");
        public static readonly SaveHelper<PersistantSaveData> PersistantSave = new(() => $"{Application.dataPath}/Saves/settings.xml");

    }
}