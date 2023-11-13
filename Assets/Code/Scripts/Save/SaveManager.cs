using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using FR8.Runtime.CodeUtility;
using UnityEngine;

namespace FR8.Runtime.Save
{
    public static class SaveManager
    {
        public static string SaveLocation => $"{Application.dataPath}/.Saves";
        public const string SaveFileExtension = ".sav";

        public static readonly SaveHelper<SaveData> ProgressionSave = new
        (
            () => $"{SaveLocation}/game.sav",
            BinarySerialize<SaveData>(),
            BinaryDeserialize<SaveData>()
        );

        public static readonly SaveHelper<PersistantSaveData> SettingsSave = new
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
        
        private static Action<Stream, T> BinarySerialize<T>()
        {
            return (s, d) => new BinaryFormatter().Serialize(s, d);
        }

        private static Func<Stream, T> BinaryDeserialize<T>()
        {
            return s => (T)new BinaryFormatter().Deserialize(s);
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
            ProgressionSave.Load();

            Debug.Log($"Loading Save...");
        }

        public static void SaveGame()
        {
            ProgressionSave.Save();
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