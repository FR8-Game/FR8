
using FMOD.Studio;
using FMODUnity;
using FR8Runtime.CodeUtility;
using UnityEngine;

namespace FR8Runtime.References
{
    public class SoundReference
    {
        public static readonly SoundReference Footsteps = "{8c3627d1-1d65-43d4-846a-3e2db6f97fff}";
        public static readonly SoundReference LeverClick = "{2e7cf040-6839-44c2-bf25-624e11105f73}";
        public static readonly SoundReference ButtonPress = "{f4b9233c-9388-4000-928b-6f5429c62867}";
        public static readonly SoundReference DoorOpen = "{4e849ba4-bf47-447c-9fde-f9da5bc1d48a}";
        public static readonly SoundReference DoorClosed = "{4e849ba4-bf47-447c-9fde-f9da5bc1d48a}";
        public static readonly SoundReference BlownFuse = "{9f2f94a0-c639-4bec-a56d-a002ab7fdd62}";
        public static readonly SoundReference LightHum = "{deb5a93a-df31-4a0b-bb87-dee26b5cd704}";
        public static readonly SoundReference PlayerShields = "{ff7980fc-84df-4dba-abf9-02f4c6bf9ed9}";
        public static readonly SoundReference PlayerDamage = "{04cad4ab-9907-4dab-b0f6-dcfde0d0b06c}";
        public static readonly SoundReference Engine = "{c793dcfe-3f65-46f2-80ae-83cee0b242b3}";
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