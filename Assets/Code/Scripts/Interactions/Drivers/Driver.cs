using System;
using FR8Runtime.Interactions.Drivers.Submodules;
using JetBrains.Annotations;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Driver : MonoBehaviour, IDriver
    {
        [SerializeField] private string key;
        [SerializeField] private string displayName;
        [SerializeField] private float defaultValue;
        
        private const float shakeAmplitude = 0.003f;
        private const float shakeFrequency = 100.0f;
        private const float shakeDecay = 12.0f;

        private Vector3 origin;
        private DriverNetwork driverNetwork;
        private float shakeTime = float.MinValue;
        
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
            Shake();
        }

        public void Shake()
        {
            shakeTime = Time.time;
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

        protected virtual void Start()
        {
            origin = transform.localPosition;
            
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

            var t = Time.time * shakeFrequency;
            var noise = new Vector3
            {
                x = Mathf.PerlinNoise(t, 10.5f) * 2.0f - 1.0f,
                y = Mathf.PerlinNoise(t, 20.5f) * 2.0f - 1.0f,
                z = Mathf.PerlinNoise(t, 30.5f) * 2.0f - 1.0f,
            } * shakeAmplitude;

            noise *= Mathf.Exp((Time.time - shakeTime) * -shakeDecay);
            transform.localPosition = origin + noise;
        }
    }
}