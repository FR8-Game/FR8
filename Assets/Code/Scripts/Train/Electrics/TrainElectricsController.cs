using System;
using System.Collections.Generic;
using UnityEngine;

namespace FR8.Train.Electrics
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrainElectricsController : MonoBehaviour
    {
        [SerializeField] private bool connected = true;
        
        private List<IElectricGenerator> generators;
        private List<IElectricDevice> devices;
        
        public float PowerDraw { get; private set; }
        public float Capacity { get; private set; }
        public float Saturation { get; private set; }
        
        private void Awake()
        {
            generators = new List<IElectricGenerator>(GetComponentsInChildren<IElectricGenerator>());
            devices = new List<IElectricDevice>(GetComponentsInChildren<IElectricDevice>());

            foreach (var e in generators) e.SetClockSpeed(0.0f);
            foreach (var e in devices) e.SetConnected(true);

            SetConnected(connected);
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
        }

        public void ResetFuze() => SetConnected(true);

        private void SetConnected(bool newFuzeState)
        {
            connected = newFuzeState;
            foreach (var e in devices)
            {
                e.SetConnected(connected);
            }
        }
    }
}
