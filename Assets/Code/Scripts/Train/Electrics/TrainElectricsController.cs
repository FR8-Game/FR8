using System.Collections.Generic;
using FR8.Interactions.Drivables;
using FR8.Interactions.Drivers;
using UnityEngine;

namespace FR8.Train.Electrics
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrainElectricsController : MonoBehaviour, IDrivable
    {
        [SerializeField] private bool connected = true;

        private DriverNetwork driverNetwork;
        private const string MainFuzeGroup = "mainFuze";
        private const string SaturationKey = "Saturation";

        private List<IElectricGenerator> generators;
        private List<IElectricDevice> devices;

        public string Key => MainFuzeGroup;
        public float PowerDraw { get; private set; }
        public float Capacity { get; private set; }
        public float Saturation { get; private set; }
        
        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();

            generators = new List<IElectricGenerator>(GetComponentsInChildren<IElectricGenerator>());
            devices = new List<IElectricDevice>(GetComponentsInChildren<IElectricDevice>());

            foreach (var e in generators) e.SetClockSpeed(0.0f);
            foreach (var e in devices) e.SetConnected(true);

            SetConnected(connected);
        }

        public void OnValueChanged(float newValue)
        {
            connected = newValue > 0.5f;
            UpdateChildren();
        }

        private void FixedUpdate()
        {
            var draw = 0.0f;
            foreach (var e in devices)
            {
                draw += e.CalculatePowerDraw();
            }

            var capacity = 0.0f;
            foreach (var e in generators) capacity += e.MaximumPowerGeneration;

            var saturation = draw / capacity;
            if (saturation > 1.01f)
            {
                saturation = 0.0f;
                SetConnected(false);
            }
            foreach (var e in generators) e.SetClockSpeed(saturation);

            PowerDraw = draw;
            Capacity = capacity;
            Saturation = saturation;
            
            driverNetwork.SetValue(SaturationKey, saturation * 100.0f);
        }

        public void ResetFuze() => SetConnected(true);

        private void SetConnected(bool connected)
        {
            driverNetwork.SetValue(Key, connected ? 1.0f : 0.0f);
        }

        private void UpdateChildren()
        {
            foreach (var e in devices)
            {
                e.SetConnected(connected);
            }
        }
    }
}
