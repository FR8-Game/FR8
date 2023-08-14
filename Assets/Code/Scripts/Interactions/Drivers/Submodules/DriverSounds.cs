using FMODUnity;
using UnityEngine;

namespace FR8.Interactions.Drivers.Submodules
{
    [System.Serializable]
    public sealed class DriverSounds
    {
        public EventReference clickEvent;
        
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
                var sound = RuntimeManager.CreateInstance(clickEvent);
                sound.set3DAttributes(gameObject.To3DAttributes());
                sound.start();
                sound.release();
            }
            
            lastValue = newValue;
        }
    }
}
