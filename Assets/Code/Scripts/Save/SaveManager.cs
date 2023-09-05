using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using FR8Runtime.CodeUtility;
using UnityEngine;

namespace FR8Runtime.Save
{
    public static class SaveManager
    {
        public static string currentSaveName = string.Empty;
        public static string SaveLocation => $"{Application.dataPath}/.Saves";
        public const string SaveFileExtension = ".sav";

        public static readonly SaveHelper<SaveData> SlotSave = new
        (
            () => $"{SaveLocation}/{(!string.IsNullOrEmpty(currentSaveName) ? currentSaveName : "Unnamed Save")}.{DateTime.UtcNow:yyyy-mm-dd-hh-mm-ss}.{SaveFileExtension}",
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
                var ext = Path.GetExtension(filename);
                if (ext != SaveFileExtension) continue;

                var name = Path.GetFileName(filename);
                var saveName = name[..name.IndexOf('.')];

                var newGroup = true;
                foreach (var group in groups)
                {
                    if (group.saveName != saveName) continue;
                    newGroup = false;
                    group.filenames.Add(filename);
                }

                if (newGroup)
                {
                    groups.Add(new SaveGroup(saveName, filename));
                }
            }

            foreach (var e in groups) e.Validate();
            
            groups.Sort((a, b) => string.Compare(b.Timestamp(), a.Timestamp()));

            return groups;
        }

        public static void LoadSave(string filename)
        {
            currentSaveName = filename;

            SceneUtility.LoadScene(SceneUtility.Scene.Game);
            Debug.Log($"Loading \"{filename}\"");
        }

        public class SaveGroup
        {
            public string saveName;
            public List<string> filenames;

            public SaveGroup(string saveName, string filename)
            {
                this.saveName = saveName;
                filenames = new List<string> { filename };
            }

            public void Validate()
            {
                filenames.Sort((a, b) => string.Compare(Timestamp(b), Timestamp(a)));
            }

            public string Timestamp() => Timestamp(filenames[0]);
            
            private string Timestamp(string filename)
            {
                var name = Path.GetFileNameWithoutExtension(filename);
                return name[(name.IndexOf('.') + 1)..];
            }
        }
    }
}