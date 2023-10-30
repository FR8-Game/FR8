using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Train.Electrics;
using UnityEngine;

namespace FR8Runtime.Train.Engine
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Locomotive))]
    public sealed class TrainEngine : MonoBehaviour, IElectricDevice
    {
        [SerializeField] private float maxSpeedKmpH = 120.0f;
        [SerializeField] private float acceleration = 8.0f;
        [SerializeField] private float maxPowerConsumption = 75.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float throttleLoadPowerSplit;
        [SerializeField] [Range(0.0f, 1.0f)] private float throttleSmoothing;
        [SerializeField] private float loadScaling = 0.02f;
        [SerializeField] [Range(0.0f, 1.0f)] private float load;
        [SerializeField] private AnimationCurve loadCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);

        [Space]
        [SerializeField] private float throttleAcceleration;

        [SerializeField] private float actualAcceleration;

        private const string ThrottleKey = "Throttle";
        private const string LoadKey = "Load";
        private const string PowerDrawKey = "PowerDraw";
        private const string CurrentKey = "Current";

        private DriverNetwork driverNetwork;
        private Locomotive locomotive;

        private float powerDraw;
        private float throttleActual;
        private float lastForwardSpeed;

        public float ThrottleInput => driverNetwork.GetValue(ThrottleKey);

        public float GetNormalizedForwardSpeed() => Mathf.InverseLerp(0.0f, maxSpeedKmpH, Mathf.Abs(locomotive.GetForwardSpeed()));

        private void Awake()
        {
            locomotive = GetComponent<Locomotive>();
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        private void Start()
        {
            driverNetwork.SetValue(PowerDrawKey, 0.0f);
            driverNetwork.SetValue(CurrentKey, 0.0f);
        }

        private void FixedUpdate()
        {
            throttleActual += (ThrottleInput - throttleActual) * (1.0f - throttleSmoothing);

            var fwdSpeed = locomotive.GetForwardSpeed();
            var connected = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f;

            CalculateLoad(fwdSpeed);

            if (connected)
            {
                ApplyDriveForce();
            }

            UpdatePowerDraw(connected);
            lastForwardSpeed = fwdSpeed;
        }

        private void UpdatePowerDraw(bool connected)
        {
            var newPowerDraw = 0.0f;
            newPowerDraw += throttleActual * maxPowerConsumption * (1.0f - throttleLoadPowerSplit);
            newPowerDraw += load * maxPowerConsumption * throttleLoadPowerSplit;

            if (locomotive.Gear == 0) newPowerDraw = 0.0f; 
            if (!connected) newPowerDraw = 0.0f; 

            powerDraw = (newPowerDraw + powerDraw) / 2.0f;
            driverNetwork.SetValue(PowerDrawKey, powerDraw);
        }

        private void CalculateLoad(float fwdSpeed)
        {
            actualAcceleration = (fwdSpeed - lastForwardSpeed) / Time.deltaTime;
            load = locomotive.Gear != 0 ? Mathf.Abs(throttleAcceleration - actualAcceleration) * loadScaling : 0.0f;
            load = Mathf.Clamp01(load);
            driverNetwork.SetValue(LoadKey, Mathf.Clamp01(load));
        }

        private void ApplyDriveForce()
        {
            var efficiency = loadCurve.Evaluate(load);

            throttleAcceleration = GetThrottleForce(throttleActual);
            locomotive.Body.AddForce(locomotive.DriverDirection * throttleAcceleration * efficiency * locomotive.ReferenceWeight);
        }

        private float GetThrottleForce(float throttle)
        {
            var fwdSpeed = locomotive.GetForwardSpeed();
            var maxSpeed = maxSpeedKmpH / 3.6f;
            return (maxSpeed - Mathf.Abs(fwdSpeed)) * acceleration * throttle * locomotive.Gear;
        }

        public float CalculatePowerDraw() => powerDraw;
    }
}