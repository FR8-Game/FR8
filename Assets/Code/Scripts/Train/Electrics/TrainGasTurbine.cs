using System;
using FR8.Interactions.Drivers;
using FR8.Utility;
using UnityEditorInternal;
using UnityEngine;

namespace FR8.Train.Electrics
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrainGasTurbine : MonoBehaviour, IElectricGenerator
    {
        [SerializeField] private float idleRpm = 1000.0f;
        [SerializeField] private float maxRpm = 10000.0f;
        [SerializeField] private float megawattsPerRevolution = 0.1f;
        
        [Space]
        [SerializeField] private float smoothTime;
        [SerializeField] private NoiseMachine engineNoise;

        private DriverGroup rmpDriver;
        
        private float targetRpm;
        private float currentRpm;
        private float velocity;

        public float MaximumPowerGeneration => maxRpm * megawattsPerRevolution;
        
        private void Awake()
        {
            var findDriver = DriverGroup.Find(gameObject);
            rmpDriver = findDriver("RPM");
        }

        private void FixedUpdate()
        {
            currentRpm = Mathf.SmoothDamp(currentRpm, targetRpm + engineNoise.Sample(Time.time, 0.5f), ref velocity, smoothTime);
            rmpDriver.SetValue(currentRpm);
        }

        public void SetClockSpeed(float percent)
        {
            targetRpm = Mathf.Max(idleRpm, maxRpm * percent);
        }
    }
}
