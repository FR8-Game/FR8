using System;
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
        [SerializeField] private float baselineGeneration = 5.0f;
        [SerializeField] private bool connected = true;

        private DriverNetwork driverNetwork;
        private const string MainFuseGroup = "mainFuze";
        private const string SaturationKey = "Saturation";

        private List<IElectricGenerator> generators;
        private List<IElectricDevice> devices;

        public string Key => MainFuseGroup;
        public Dictionary<string, bool> FuseGroups = new();

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
            foreach (var e in devices) e.SetConnected(true);

            connected = !connected;
            SetConnected(!connected);
        }

        public void OnValueChanged(float newValue)
        {
            SetConnected(newValue > 0.5f);
        }

        private void FixedUpdate()
        {
            UpdateChildren();

            var saturation = 0.0f;
            var clockSpeed = 0.0f;
            var draw = 0.0f;

            var capacity = baselineGeneration;
            foreach (var e in generators) capacity += e.MaximumPowerGeneration;

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
                SetMainFuse(false);
                FuseBlown?.Invoke();
            }

            foreach (var e in generators) e.SetClockSpeed(clockSpeed);

            PowerDraw = draw;
            Capacity = capacity;
            Saturation = saturation;

            driverNetwork.SetValue(SaturationKey, saturation * 100.0f);
        }

        public void ResetFuze() => SetMainFuse(true);

        public void SetConnected(bool connected)
        {
            if (this.connected == connected) return;

            this.connected = connected;
            foreach (var e in generators)
            {
                e.ChangedFuseState(connected);
            }
        }

        public void SetMainFuse(bool connected)
        {
            driverNetwork.SetValue(Key, connected ? 1.0f : 0.0f);
            foreach (var e in generators)
            {
                e.ChangedFuseState(connected);
            }
        }

        public bool GetMainFuse() => connected;

        private void UpdateChildren()
        {
            foreach (var e in devices)
            {
                e.SetConnected(GetFuse(e.FuseGroup));
            }
        }

        private static string Simplify(string str) => str?.Trim().ToLower().Replace(" ", "");

        public void SetFuse(string fuseName, bool state)
        {
            fuseName = Simplify(fuseName);

            if (FuseGroups.ContainsKey(fuseName)) FuseGroups[fuseName] = state;
            else FuseGroups.Add(fuseName, state);
        }

        public bool GetFuse(string fuseName)
        {
            fuseName = Simplify(fuseName);

            if (!connected) return false;
            if (string.IsNullOrEmpty(fuseName)) return true;

            return FuseGroups.ContainsKey(fuseName) && FuseGroups[fuseName];
        }
    }
}