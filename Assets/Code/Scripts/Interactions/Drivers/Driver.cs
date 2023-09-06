using System;
using FR8Runtime.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Driver : MonoBehaviour, IDriver
    {
        [SerializeField] private string key;
        [SerializeField] private string displayName;
        [SerializeField] private float defaultValue;
        
        private DriverNetwork driverNetwork;
        
        public virtual bool CanInteract => true;
        public virtual string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public virtual string DisplayValue => $"{Mathf.RoundToInt(Value * 100.0f)}%";
        
        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new NotImplementedException();
        
        public float Value { get; private set; }
        public string Key => key;
        
        public virtual void OnValueChanged(float newValue)
        {
            Value = newValue;
        }

        protected virtual void SetValue(float newValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                OnValueChanged(newValue);
                return;
            }

            if (driverNetwork) driverNetwork.SetValue(key, newValue);
        }

        public abstract void Nudge(int direction);

        public abstract void BeginInteract(GameObject interactingObject);

        public abstract void ContinueInteract(GameObject interactingObject);

        protected virtual void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        protected void Start()
        {
            SetValue(defaultValue);
            OnValueChanged(defaultValue);
        }
        
        protected virtual void FixedUpdate()
        {
            var newValue = driverNetwork.GetValue(key);
            
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (newValue != Value)
            {
                OnValueChanged(newValue);
            }
        }
    }
}