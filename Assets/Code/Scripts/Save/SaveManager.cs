using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using FR8Runtime.CodeUtility;
using UnityEditor;
using UnityEngine;

namespace FR8Runtime.Save
{
    public static class SaveManager
    {
        public static string saveName = "New Save";
        public static string SaveLocation => $"{Application.dataPath}/.Saves";
        public const string SaveFileExtension = ".sav";

        public static string SaveFilename => $"{SaveLocation}/{saveName}.sav";

        public static readonly SaveHelper<SaveData> SlotSave = new
        (
            () => SaveFilename,
            XMLSerialize<SaveData>(),
            XMLDeserialize<SaveData>()
        );

        public static readonly SaveHelper<PersistantSaveData> PersistantSave = new
        (
            () => $"{SaveLocation}/settings.xml",
            XMLSerialize<PersistantSaveData>(),
            XMLDeserialize<PersistantSaveData>()
        );

        private static Action<Stream, T> XMLSerialize<T>()
        {
            return (s, d) => new XmlSerializer(typeof(T)).Serialize(s, d);
        }

        private static Func<Stream, T> XMLDeserialize<T>()
        {
            return (s) => (T)new XmlSerializer(typeof(T)).Deserialize(s);
        }

        public static List<SaveGroup> GetAllSaveGroups()
        {
            var groups = new List<SaveGroup>();

            foreach (var filename in Directory.EnumerateFiles(SaveLocation))
            {
                if (!File.Exists(filename)) continue;
                
                var ext = Path.GetExtension(filename);
                if (ext != SaveFileExtension) continue;

                var name = Path.GetFileNameWithoutExtension(filename);
                var separation = name.IndexOf('.');
                var groupName = separation != -1 ? name[..separation] : "Uncategorized";

                var newGroup = true;
                foreach (var group in groups)
                {
                    if (group.groupName != groupName) continue;
                    newGroup = false;
                    group.saveNames.Add(name);
                }

                if (newGroup)
                {
                    groups.Add(new SaveGroup(groupName, name));
                }
            }

            foreach (var e in groups) e.Validate();

            groups.Sort((a, b) => string.Compare(b.groupName, a.groupName));

            return groups;
        }

        public static void LoadSave()
        {
            SceneUtility.LoadScene(SceneUtility.Scene.Game);
            SlotSave.Load();

            Debug.Log($"Loading \"{saveName}\"");
        }

        public static void SaveGame()
        {
            SlotSave.Save();
        }

        public class SaveGroup
        {
            public string groupName;
            public List<string> saveNames;

            public SaveGroup(string groupName, string filename)
            {
                this.groupName = groupName;
                saveNames = new List<string> { filename };
            }

            public void Validate()
            {
                saveNames.Sort((a, b) => string.Compare(b, a));
            }
        }
    }
}