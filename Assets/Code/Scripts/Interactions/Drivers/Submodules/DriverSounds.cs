using FMODUnity;
using FR8Runtime.CodeUtility;
using FR8Runtime.References;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers.Submodules
{
    [System.Serializable]
    public sealed class DriverSounds
    {
        private GameObject gameObject;
        private float lastValue;

        public void Awake(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public void SetValue(float newValue, int steps)
        {
            var index = Mathf.RoundToInt(newValue * steps);
            var lastIndex = Mathf.RoundToInt(lastValue * steps);

            if (index != lastIndex)
            {
                SoundUtility.PlayOneShot(SoundReference.LeverClick);
            }

            lastValue = newValue;
        }
    }
}