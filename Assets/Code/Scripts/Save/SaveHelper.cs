using System;
using System.IO;
using UnityEngine;

namespace FR8Runtime.Save
{
    public class SaveHelper<T> where T : class, new()
    {
        public T data;
        private Func<string> filename;
        
        public Action<Stream, T> serializeCallback;
        public Func<Stream, T> deserializeCallback;
        
        public event Action DataChangedEvent;
        
        internal SaveHelper(Func<string> filename, Action<Stream, T> serializeCallback, Func<Stream, T> deserializeCallback)
        {
            this.filename = filename;

            this.serializeCallback = serializeCallback;
            this.deserializeCallback = deserializeCallback;
        }
        
        public T GetOrLoad()
        {
            if (data == null) Load();
            return data;
        }

        public bool HasSave()
        {
            var filename = this.filename();
            return File.Exists(filename);
        }
        
        public void Load()
        {
            var filename = this.filename();
            if (!HasSave())
            {
                Debug.Log($"No Persistant Save Data was found at \"{filename}\", creating new one.");
                data = new T();
                Save();
                return;
            }
            
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                data = deserializeCallback(stream);
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
            
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                serializeCallback(stream, data);
            }
            
            DataChangedEvent?.Invoke();
            Debug.Log($"Finished Saving Persistant data to \"{filename}\"");
        }
    }
}