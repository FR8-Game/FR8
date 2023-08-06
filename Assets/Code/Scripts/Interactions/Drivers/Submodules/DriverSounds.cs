using FMODUnity;
using UnityEngine;

namespace FR8.Interactions.Drivers.Submodules
{
    [System.Serializable]
    public sealed class DriverSounds
    {
        private float lastValue;
        private StudioEventEmitter emitter;

        public void Awake(GameObject gameObject)
        {
            emitter = gameObject.GetComponent<StudioEventEmitter>();
        }
        
        public void SetValue(float newValue, int steps)
        {
            if (!emitter) return;
            
            var index = Mathf.RoundToInt(newValue * steps);
            var lastIndex = Mathf.RoundToInt(lastValue * steps);

            if (index != lastIndex) emitter.Play();

            lastValue = newValue;
        }
    }
}
