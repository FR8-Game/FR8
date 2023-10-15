using System;
using System.Collections.Generic;
using FR8Runtime.Contracts;
using FR8Runtime.Contracts.Predicates;
using FR8Runtime.Interactions.Drivers.Submodules;
using FR8Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Driver : MonoBehaviour, IDriver
    {
        [SerializeField] private float defaultValue;

        private const float ShakeAmplitude = 0.0015f;
        private const float ShakeFrequency = 100.0f;
        private const float ShakeDecay = 12.0f;

        private bool highlighted;

        private Vector3 origin;
        private DriverNetwork driverNetwork;
        private float shakeTime = float.MinValue;

        public virtual bool CanInteract => true;
        public virtual string DisplayName => name;
        public virtual string DisplayValue => $"{Mathf.RoundToInt(Value * 100.0f)}%";

        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new NotImplementedException();
        public IEnumerable<Renderer> Visuals { get; private set; }

        public float Value { get; private set; }
        public string Key => name;

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
            if (string.IsNullOrEmpty(Key))
            {
                OnValueChanged(newValue);
                return;
            }

            if (driverNetwork) driverNetwork.SetValue(Key, newValue);
        }

        public abstract void Nudge(int direction);

        public abstract void BeginInteract(GameObject interactingObject);

        public abstract void ContinueInteract(GameObject interactingObject);

        protected virtual void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();
            Visuals = GetComponentsInChildren<Renderer>();
        }

        protected virtual void Start()
        {
            origin = transform.localPosition;

            //if (!driverNetwork.HasValue(key))
            {
                SetValue(defaultValue);
                OnValueChanged(defaultValue);
            }
        }

        protected virtual void FixedUpdate()
        {
            UpdateValueIfChanged();
            ShakeOnInteract();
            Highlight();
        }

        private void Highlight()
        {
            var isHighlighted = false;
            foreach (var contract in Contract.ActiveContracts)
            {
                checkPredicate(contract.ActivePredicate);
            }

            if (isHighlighted != highlighted)
            {
                if (isHighlighted)
                {
                    SelectionOutlinePass.Add(Visuals);
                }
                else
                {
                    SelectionOutlinePass.Remove(Visuals);
                }
                highlighted = isHighlighted;
            }

            void checkPredicate(ContractPredicate predicate)
            {
                switch (predicate)
                {
                    case PredicateGroup group:
                    {
                        foreach (var e in group)
                        {
                            checkPredicate(e);
                        }

                        break;
                    }
                    case DriverNetworkPredicate driverPredicate:
                    {
                        if (!DriverNetwork.CompareKeys(driverPredicate.Key, Key)) break;

                        isHighlighted = true;
                        return;
                    }
                }
            }
        }

        private void UpdateValueIfChanged()
        {
            var newValue = driverNetwork.GetValue(Key);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (newValue != Value)
            {
                OnValueChanged(newValue);
            }
        }

        private void ShakeOnInteract()
        {
            var t = Time.time * ShakeFrequency;
            var noise = new Vector3
            {
                x = Mathf.PerlinNoise(t, 10.5f) * 2.0f - 1.0f,
                y = Mathf.PerlinNoise(t, 20.5f) * 2.0f - 1.0f,
                z = Mathf.PerlinNoise(t, 30.5f) * 2.0f - 1.0f,
            } * ShakeAmplitude;

            noise *= Mathf.Exp((Time.time - shakeTime) * -ShakeDecay);
            transform.localPosition = origin + noise;
        }
    }
}