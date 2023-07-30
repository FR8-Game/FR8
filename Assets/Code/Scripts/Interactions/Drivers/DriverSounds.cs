using System;
using FMODUnity;
using FR8.Interactions.Drivables;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StudioEventEmitter))]
    public sealed class DriverSounds : MonoBehaviour, IDrivable
    {
        [SerializeField] private int steps;

        private float lastValue;
        private StudioEventEmitter emitter;

        private void Awake()
        {
            emitter = GetComponent<StudioEventEmitter>();
        }

        public void SetValue(DriverGroup group, float value)
        {
            var index = Mathf.RoundToInt(value * steps);
            var lastIndex = Mathf.RoundToInt(lastValue * steps);

            if (index != lastIndex) emitter.Play();

            lastValue = value;
        }
    }
}
