using FR8.Interactions.Drivers;
using FR8.Utility;
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

        private const string RpmKey = "RPM";

        private DriverNetwork driverNetwork;
        
        private float targetRpm;
        private float currentRpm;
        private float velocity;

        public float MaximumPowerGeneration => maxRpm * megawattsPerRevolution;
        
        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        private void FixedUpdate()
        {
            currentRpm = Mathf.SmoothDamp(currentRpm, targetRpm + engineNoise.Sample(Time.time, 0.5f), ref velocity, smoothTime);
            driverNetwork.SetValue(RpmKey, currentRpm);
        }

        public void SetClockSpeed(float percent)
        {
            targetRpm = Mathf.Max(idleRpm, maxRpm * percent);
        }
    }
}
