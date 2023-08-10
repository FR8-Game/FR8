using FR8.Interactions.Drivers;
using UnityEngine;

namespace FR8.Train.Electrics
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrainGasTurbine : MonoBehaviour, IElectricGenerator
    {
        [SerializeField] private float idleRpm = 1000.0f;
        [SerializeField] private float maxRpm = 10000.0f;
        [SerializeField] private float stallRpm = 700.0f;
        
        [Space]
        [SerializeField] private float maxPowerProduction = 200.0f;
        
        [Space]
        [SerializeField] private float fuelCapacity = 20.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float fuelLevel = 1.0f;

        [Space]
        [SerializeField] private float smoothTime;
        [SerializeField] private Utility.NoiseMachine engineNoise;

        private const string IgnitionKey = "Ignition";
        private const string FuelKey = "Fuel";
        private const string RpmKey = "RPM";

        private DriverNetwork driverNetwork;

        private float targetRpm;
        private float currentRpm;
        private float velocity;

        public bool Running => fuelLevel > 0.0f && currentRpm > stallRpm;
        public float MaximumPowerGeneration => Running ? maxPowerProduction : 0.0f;

        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        private void FixedUpdate()
        {
            var t = targetRpm + engineNoise.Sample(Time.time, 0.5f);

            if (Running)
            {
                var fuelConsumption = currentRpm / maxRpm;
                fuelLevel -= fuelConsumption / (fuelCapacity * 60.0f) * Time.deltaTime;
            }
            else if (driverNetwork.Read(IgnitionKey) > 0.5f)
            {
                t = idleRpm;
            }
            else
            {
                t = 0.0f;
            }

            currentRpm = Mathf.SmoothDamp(currentRpm, t, ref velocity, smoothTime);
            driverNetwork.SetValue(RpmKey, currentRpm);
            driverNetwork.SetValue(FuelKey, fuelLevel * 100.0f);
        }

        public void SetClockSpeed(float percent)
        {
            targetRpm = Mathf.Max(idleRpm, maxRpm * percent);
        }
    }
}