using System;
using System.Collections.Generic;
using FMODUnity;
using FR8Runtime.CodeUtility;
using FR8Runtime.Interactions.Drivers;
using UnityEngine;

namespace FR8Runtime.Train.Electrics
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrainElectricsController : MonoBehaviour
    {
        [SerializeField] private float baselineGeneration = 5.0f;
        [SerializeField] private EventReference fuseBlownSound;

        private DriverNetwork driverNetwork;
        
        public const string MainFuse = "mainFuse";
        public const string SaturationKey = "Saturation";

        private List<IElectricGenerator> generators;
        private List<IElectricDevice> devices;

        public float PowerDraw { get; private set; }
        public float Capacity { get; private set; }
        public float Saturation { get; private set; }

        public event Action FuseBlown;

        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();

            generators = new List<IElectricGenerator>(GetComponentsInChildren<IElectricGenerator>());
            devices = new List<IElectricDevice>(GetComponentsInChildren<IElectricDevice>());

            foreach (var e in generators) e.SetClockSpeed(0.0f);

            SetConnected(false);
        }

        public void OnValueChanged(float newValue)
        {
            SetConnected(newValue > 0.5f);
        }

        private void FixedUpdate()
        {
            var saturation = 0.0f;
            var clockSpeed = 0.0f;
            var draw = 0.0f;

            var capacity = baselineGeneration;
            foreach (var e in generators) capacity += e.MaximumPowerGeneration;

            var connected = driverNetwork.GetValue(MainFuse) > 0.5f;
            
            if (connected)
            {
                draw = 0.0f;
                foreach (var e in devices)
                {
                    draw += e.CalculatePowerDraw();
                }

                saturation = draw / capacity;
                clockSpeed = draw / (capacity - baselineGeneration);
            }

            if (saturation > 1.01f)
            {
                saturation = 0.0f;
                clockSpeed = 0.0f;
                SetConnected(false);
                FuseBlown?.Invoke();
                SoundUtility.PlayOneShot(fuseBlownSound, gameObject);
            }

            foreach (var e in generators) e.SetClockSpeed(clockSpeed);

            PowerDraw = draw;
            Capacity = capacity;
            Saturation = saturation;

            driverNetwork.SetValue(SaturationKey, saturation * 100.0f);
        }

        public bool GetConnected() => driverNetwork.GetValue(MainFuse) > 0.5f;
        
        public void SetConnected(bool connected)
        {
            driverNetwork.SetValue(MainFuse, connected ? 1.0f : 0.0f);
        }
    }
}