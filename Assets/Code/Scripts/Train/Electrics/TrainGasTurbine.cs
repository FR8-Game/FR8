using FMOD.Studio;
using FMODUnity;
using FR8Runtime.Interactions.Drivers;
using UnityEngine;

namespace FR8Runtime.Train.Electrics
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrainGasTurbine : MonoBehaviour, IElectricGenerator
    {
        public float idleRpm = 1000.0f;
        public float maxRpm = 10000.0f;
        public float stallRpm = 700.0f;

        [Space]
        public float maxPowerProduction = 200.0f;

        [Space]
        public float fuelCapacity = 20.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float fuelLevel = 1.0f;

        [Space]
        [SerializeField] private float smoothTime;

        [SerializeField] private CodeUtility.NoiseMachine engineNoise;

        [Space]
        [SerializeField] private EventReference engineSounds;

        private const string IgnitionKey = "Ignition";
        private const string FuelKey = "Fuel";
        private const string RpmKey = "RPM";

        private DriverNetwork driverNetwork;
        private EventInstance soundInstance;

        private float targetRpm;
        private float currentRpm;
        private float velocity;
        private bool wasRunning;
        public bool running;

        public float CurrentRpm => currentRpm;
        public float MaximumPowerGeneration => running ? maxPowerProduction : 0.0f;
        public float FuelLevel => fuelLevel;

        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();
            soundInstance = RuntimeManager.CreateInstance(engineSounds);
        }

        private void OnDisable()
        {
            soundInstance.stop(STOP_MODE.IMMEDIATE);
            soundInstance.release();
        }

        private void FixedUpdate()
        {
            var t = targetRpm + engineNoise.Sample(Time.time, 0.5f);

            var connected = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f;
            if (!connected) t = 0.0f;

            running = fuelLevel > 0.0f && currentRpm > stallRpm;
            if (driverNetwork.GetValue(IgnitionKey) > 0.5f)
            {
                running = true;
            }
            if (!running) t = 0.0f;
            
            UpdateSounds();

            currentRpm = Mathf.SmoothDamp(currentRpm, t, ref velocity, smoothTime);
            driverNetwork.SetValue(RpmKey, currentRpm);
            driverNetwork.SetValue(FuelKey, fuelLevel * 100.0f);

            var fuelConsumption = currentRpm / maxRpm;
            fuelLevel -= fuelConsumption / (fuelCapacity * 60.0f) * Time.deltaTime;

            wasRunning = running;
        }

        private void UpdateSounds()
        {
            if (running != wasRunning)
            {
                if (running)
                {
                    soundInstance.start();
                    Debug.Log("start");
                }
                else
                {
                    soundInstance.stop(STOP_MODE.ALLOWFADEOUT);
                    Debug.Log("stop");
                }
            }

            soundInstance.setParameterByName("RPM", currentRpm);
        }

        public void SetClockSpeed(float percent)
        {
            if (!running)
            {
                targetRpm = 0.0f;
                return;
            }
            
            targetRpm = Mathf.Max(idleRpm, maxRpm * percent);
        }

        public void Refuel(float rate)
        {
            fuelLevel += rate / fuelCapacity * Time.deltaTime;
            fuelLevel = Mathf.Clamp01(fuelLevel);
        }
    }
}