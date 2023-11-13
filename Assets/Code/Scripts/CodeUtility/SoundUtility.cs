using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace FR8.Runtime.CodeUtility
{
    public static class SoundUtility
    {
        public static bool log = false;

        private static GameObject emitter;

        public static void PlayOneShot(string guid)
        {
            PlayOneShot(guid, _ => { });
        }

        private static void PlayOneShot(string guid, Action<EventInstance> callback)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                LogGeneric("Null", "could not be found!", Debug.LogError);
                return;
            }
            
            var reference = FromGuid(guid);
            var sound = RuntimeManager.CreateInstance(reference);
            if (!sound.isValid())
            {
                LogGeneric(guid, "is not valid!", Debug.LogError);
                return;
            }

            callback?.Invoke(sound);
            sound.start();
            sound.release();
        }
        
        private static EventReference FromGuid(string guid) => new() { Guid = string.IsNullOrWhiteSpace(guid) ? new GUID() : new GUID(Guid.Parse(guid)) };

        public static void PlayOneShot(string guid, GameObject emitter)
        {
            SoundUtility.emitter = emitter;
            PlayOneShot(guid, sound => sound.set3DAttributes(emitter.To3DAttributes()));
        }

        public static void PlayOnChange(bool state, bool newState, string guid) => PlayOnChange(state, newState, guid, guid);

        public static void PlayOnChange(bool state, bool newState, string onGuid, string offGuid)
        {
            if (newState == state) return;
            var guid = newState ? onGuid : offGuid;
            PlayOneShot(guid);
        }

        public static void LogGeneric(string guid, string message, Action<string, Object> debugCallback = null)
        {
            if (!log) return;
            if (debugCallback == null) debugCallback = Debug.Log;

            var ex = new Exception($"Sound \"{guid}\" {message}!");
            Debug.LogWarning(ex, emitter);
        }

        public static EventInstance Instance(string guid) => string.IsNullOrWhiteSpace(guid) ? new EventInstance() : RuntimeManager.CreateInstance(FromGuid(guid));
    }
}