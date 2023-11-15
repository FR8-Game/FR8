using FMOD.Studio;
using FMODUnity;
using FR8.Runtime.CodeUtility;
using UnityEngine;

namespace FR8.Runtime.References
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
        public static readonly SoundReference TitleMusic = "{9a601097-2e7b-4124-9c81-224c45cb4e1c}";
        public static readonly SoundReference Ambiance = "{fbcafde3-4539-43e2-822e-b75ec64c52f6}";
        public static readonly SoundReference Stereo = "";
        public static readonly SoundReference Radio = "{10d74e18-1e48-41e7-91d9-6052a7e6d2b4}";
        public static readonly SoundReference TrainBrake = "";
        public static readonly SoundReference TrainChangeTracks = "";
        public static readonly SoundReference MenuClick = "{2ba267b2-f7a1-467c-956d-ca3191fa2679}";
        public static readonly SoundReference Wind = "";
        public static readonly SoundReference WindowWipers = "";
        public static readonly SoundReference FuzeDoor = "{9a6df018-805a-4ee8-ab6d-7efd96814aee}";
        public static readonly SoundReference PlayerSit = "{8e1d9b9e-f82b-4572-8acd-fa6dd0177d7e}";

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