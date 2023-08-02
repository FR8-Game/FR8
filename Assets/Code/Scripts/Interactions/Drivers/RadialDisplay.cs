using System;
using FR8.Interactions.Drivables;
using UnityEngine;

namespace FR8
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class RadialDisplay : MonoBehaviour, IDrivable
    {
        [SerializeField] private string key;

        [Space]
        [SerializeField] private DriverDisplayText displayText;
        [SerializeField] private DriverDisplayUI displayUI;
        
        public string DisplayValue => Mathf.RoundToInt(Value * 100.0f).ToString();
        public float Value { get; private set; }
        public string Key => key;
        
        public void OnValueChanged(float newValue)
        {
            Value = newValue;
            
            displayText.SetValue(newValue);
            displayUI.SetValue(newValue);
        }

        private void Awake()
        {
            displayText.Awake();
        }

        private void OnValidate()
        {
            displayUI.OnValidate();
        }
    }
}