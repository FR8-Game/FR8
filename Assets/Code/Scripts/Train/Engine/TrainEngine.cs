using FR8.Runtime.Interactions.Drivers;
using FR8.Runtime.Train.Electrics;
using UnityEngine;

namespace FR8.Runtime.Train.Engine
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Locomotive))]
    public sealed class TrainEngine : MonoBehaviour, IElectricDevice
    {
        public float throttleAcceleration;
        public float actualAcceleration;
        [Range(0.0f, 1.0f)] public float load;

        [Space]
        public bool overrideThrottle;
        [Range(-1.0f, 1.0f)] public float overrideThrottleValue;

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
        public LocomotiveSettings Settings => locomotive.locomotiveSettings;

        public float GetNormalizedForwardSpeed() => Mathf.InverseLerp(0.0f, Settings.maxSpeedKmpH, Mathf.Abs(locomotive.GetForwardSpeed()));

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
            #if UNITY_EDITOR
            if (overrideThrottle)
            {
                locomotive.Body.velocity = locomotive.DriverDirection * overrideThrottleValue * (Settings.maxSpeedKmpH / 3.6f);
                return;
            }
            #endif
            
            throttleActual += (ThrottleInput - throttleActual) * (1.0f - Settings.throttleSmoothing);

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
            newPowerDraw += throttleActual * Settings.maxPowerConsumption * (1.0f - Settings.throttleLoadPowerSplit);
            newPowerDraw += load * Settings.maxPowerConsumption * Settings.throttleLoadPowerSplit;

            if (locomotive.Gear == 0) newPowerDraw = 0.0f;
            if (!connected) newPowerDraw = 0.0f;

            powerDraw = (newPowerDraw + powerDraw) / 2.0f;
            driverNetwork.SetValue(PowerDrawKey, powerDraw);
        }

        private void CalculateLoad(float fwdSpeed)
        {
            if (locomotive.Gear == 0)
            {
                load = 0.0f;
            }
            
            actualAcceleration = (fwdSpeed - lastForwardSpeed) / Time.deltaTime;
            load = locomotive.Gear != 0 ? Mathf.Abs(throttleAcceleration - actualAcceleration) * Settings.loadScaling : 0.0f;
            load = Mathf.Clamp01(load);
            driverNetwork.SetValue(LoadKey, Mathf.Clamp01(load));
        }

        private void ApplyDriveForce()
        {
            var efficiency = Settings.loadCurve.Evaluate(load);

            throttleAcceleration = GetThrottleForce(throttleActual) * locomotive.Gear * efficiency;
            locomotive.Body.AddForce(locomotive.DriverDirection * throttleAcceleration * efficiency * locomotive.referenceWeight);
        }

        private float GetThrottleForce(float throttle)
        {
            var fwdSpeed = locomotive.GetForwardSpeed();
            var maxSpeed = Settings.maxSpeedKmpH / 3.6f;
            return (maxSpeed - Mathf.Abs(fwdSpeed)) * Settings.acceleration * throttle;
        }

        public float CalculatePowerDraw() => powerDraw;

        private void OnValidate()
        {
            if (Mathf.Abs(overrideThrottleValue) < 0.05f)
            {
                overrideThrottleValue = 0.0f;
            }
        }
    }
}