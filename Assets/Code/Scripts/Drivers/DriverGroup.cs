using System.Collections.Generic;
using FR8.Drivables;
using UnityEngine;
using UnityEngine.Serialization;

namespace FR8.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class DriverGroup : MonoBehaviour
    {
        [SerializeField] private float defaultValue;

        private List<IDriver> drivers = new();
        private List<IDrivable> drivables = new();

        public string GroupName => gameObject.name;
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