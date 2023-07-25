using System.Collections.Generic;
using FR8.Drivables;
using UnityEngine;

namespace FR8.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class DriverGroup : MonoBehaviour
    {
        [SerializeField] private string displayName;
        [SerializeField] private float defaultValue;

        private List<IDriver> drivers = new();
        private List<IDrivable> drivables = new();

        public string GroupName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
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

        public void SetValue(float newValue)
        {
            Value = newValue;
            
            foreach (var driver in drivers)
            {
                driver.ValueUpdated();
            }

            foreach (var drivable in drivables)
            {
                drivable.SetValue(this, Value);
            }
        }
    }
}