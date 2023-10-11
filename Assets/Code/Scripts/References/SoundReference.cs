
using FMOD.Studio;
using FMODUnity;
using FR8Runtime.CodeUtility;
using UnityEngine;

namespace FR8Runtime.References
{
    public class SoundReference
    {
        public static readonly SoundReference Footsteps = "";
        public static readonly SoundReference LeverClick = "";
        public static readonly SoundReference ButtonPress = "";
        public static readonly SoundReference DoorOpen = "";
        public static readonly SoundReference DoorClosed = "";
        public static readonly SoundReference BlownFuse = "";
        public static readonly SoundReference LightHum = "";
        public static readonly SoundReference PlayerShields = "";
        public static readonly SoundReference PlayerDamage = "";
        public static readonly SoundReference Engine = "";
        public static readonly SoundReference TitleMusic = "";
        public static readonly SoundReference Ambiance = "";
        public static readonly SoundReference Stereo = "";
        public static readonly SoundReference Radio = "";
        public static readonly SoundReference TrainBrake = "";
        public static readonly SoundReference TrainChangeTracks = "";
        public static readonly SoundReference MenuClick = "";
        public static readonly SoundReference Wind = "";
        public static readonly SoundReference WindowWipers = "";

        private string guid;

        public SoundReference(string guid)
        {
            this.guid = guid;
        }

        public static implicit operator SoundReference(string guid) => new(guid);
        public static implicit operator string(SoundReference sound) => sound.guid;

        public void PlayOneShot() => SoundUtility.PlayOneShot(guid);
        public void PlayOneShot(GameObject emitter) => SoundUtility.PlayOneShot(guid, emitter);
        public EventInstance Instance() => SoundUtility.Instance(guid);
        public EventInstance InstanceAndStart(GameObject gameObject = null)
        {
            var instance = Instance();
            instance.start();
            if (gameObject) instance.set3DAttributes(gameObject.To3DAttributes());
            return instance;
        }
    }
}