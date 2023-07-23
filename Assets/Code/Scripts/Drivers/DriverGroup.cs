using System.Collections.Generic;
using FR8.Drivables;
using UnityEngine;

namespace FR8.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class DriverGroup : MonoBehaviour
    {
        [SerializeField] private float defaultValue;
        [SerializeField] private int steps = 0;
        [SerializeField] private bool forceStep = false;
        [SerializeField] private bool limit = true;
        [SerializeField] private float min = 0.0f;
        [SerializeField] private float max = 1.0f;

        private List<IDriver> drivers = new();
        private List<IDrivable> drivables = new();

        public float Value => Mathf.Lerp(min, max, NormalizedValue);
        public float NormalizedValue { get; private set; }

        private void Awake()
        {
            drivers.AddRange(GetComponentsInChildren<IDriver>());
            drivables.AddRange(GetComponentsInChildren<IDrivable>());

            foreach (var driver in drivers)
            {
                driver.SetDriverGroup(this);
            }
        }

        private void Start()
        {
            SetNormalizedValue(defaultValue);
        }

        public void SetValue(float newValue)
        {
            SetNormalizedValue(Mathf.InverseLerp(min, max, newValue));
        }
        
        public void SetNormalizedValue(float newValue)
        {
            NormalizedValue = ValidateValue(newValue);

            foreach (var driver in drivers)
            {
                driver.ValueUpdated();
            }

            foreach (var drivable in drivables)
            {
                drivable.SetValue(Value);
            }
        }

        public void Nudge(int direction)
        {
            if (direction > 1) direction = 1;
            if (direction < -1) direction = -1;

            var normalizedValue = Mathf.InverseLerp(min, max, Value);
            var steppedValue = Mathf.RoundToInt(normalizedValue * steps);
            steppedValue += direction;
            
            var newValue = steppedValue / (float)steps;
            SetNormalizedValue(newValue);
        }

        private float ValidateValue(float newValue)
        {
            if (steps > 0 && forceStep)
            {
                newValue = Mathf.Round(newValue * steps) / steps;
            }

            if (limit)
            {
                newValue = Mathf.Clamp01(newValue);
            }

            return newValue;
        }
    }
}