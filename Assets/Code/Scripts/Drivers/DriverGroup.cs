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
        [SerializeField] private bool limit = true;
        [SerializeField] private float min = 0.0f;
        [SerializeField] private float max = 1.0f;

        private List<IDriver> drivers = new();
        private List<IDrivable> drivables = new();

        public float Value { get; private set; }

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
            SetValue(defaultValue);
        }

        public float SetValue(float newValue)
        {
            Value = ValidateValue(newValue);

            foreach (var driver in drivers)
            {
                driver.ValueUpdated();
            }

            foreach (var drivable in drivables)
            {
                drivable.SetValue(Value);
            }
            
            return Value;
        }

        private float ValidateValue(float newValue)
        {
            if (steps > 0)
            {
                newValue = Mathf.Round(newValue * steps) / steps;
            }

            if (limit)
            {
                newValue = Mathf.Clamp(newValue, min, max);
            }

            return newValue;
        }
    }
}