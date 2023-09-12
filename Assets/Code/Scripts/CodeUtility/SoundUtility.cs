using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FR8Runtime.CodeUtility
{
    public static class SoundUtility
    {
        public static bool logErrors = false;

        private static string path;
        private static GameObject emitter;

        public static void PlayOneShot(string path)
        {
            PlayOneShot(path, _ => { });
        }

        public static void PlayOneShot(string path, GameObject emitter)
        {
            SoundUtility.emitter = emitter;
            PlayOneShot(path, sound => sound.set3DAttributes(emitter.To3DAttributes()));
        }
        
        private static void PlayOneShot(string path, Action<EventInstance> callback)
        {
            SoundUtility.path = path;
            
            var reference = EventReference.Find(path);
            if (reference.IsNull || reference.Guid == new GUID())
            {
                LogGeneric("could not be found!");
                return;
            }
            
            var sound = RuntimeManager.CreateInstance(reference);
            if (!sound.isValid())
            {
                LogGeneric("is not valid!");
                return;
            }

            callback?.Invoke(sound);
            sound.start();
            sound.release();
        }

        public static void LogGeneric(string reason)
        {
            if (!logErrors) return;

            var ex = new Exception($"Sound \"{path}\" {reason}!");
            Debug.LogWarning(ex, emitter);
        }
    }
}