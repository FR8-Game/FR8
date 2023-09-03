using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FR8Runtime.Save
{
    public class SaveHelper<T> where T : class, new()
    {
        public T data;
        private Func<string> filename;

        public event Action DataChangedEvent;

        internal SaveHelper(Func<string> filename)
        {
            this.filename = filename;
        }
        
        public T GetOrLoad()
        {
            if (data == null) Load();
            return data;
        }
        
        public void Load()
        {
            var filename = this.filename();
            if (!File.Exists(filename))
            {
                Debug.Log($"No Persistant Save Data was found at \"{filename}\", creating new one.");
                data = new T();
                Save();
                return;
            }
            
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var formatter = new XmlSerializer(typeof(T));
                data = (T)formatter.Deserialize(stream);
            }
            
            Debug.Log($"Finished Reading Persistant data from \"{filename}\"");
        }

        public void Save()
        {
            var filename = this.filename();
            var dir = Path.GetDirectoryName(filename);
            if (string.IsNullOrEmpty(dir)) throw new Exception();
            
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            using (var stream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var formatter = new XmlSerializer(typeof(T));
                formatter.Serialize(stream, data);
            }
            
            DataChangedEvent?.Invoke();
            Debug.Log($"Finished Saving Persistant data to \"{filename}\"");
        }
    }
}