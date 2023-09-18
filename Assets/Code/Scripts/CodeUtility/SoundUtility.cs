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

        private static EventReference reference;
        private static GameObject emitter;

        public static void PlayOneShot(EventReference reference)
        {
            PlayOneShot(reference, _ => { });
        }

        public static void PlayOneShot(EventReference reference, GameObject emitter)
        {
            SoundUtility.emitter = emitter;
            PlayOneShot(reference, sound => sound.set3DAttributes(emitter.To3DAttributes()));
        }
        
        private static void PlayOneShot(EventReference reference, Action<EventInstance> callback)
        {
            SoundUtility.reference = reference;
            
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

            var ex = new Exception($"Sound \"{reference.Guid}\" {reason}!");
            Debug.LogWarning(ex, emitter);
        }
    }
}