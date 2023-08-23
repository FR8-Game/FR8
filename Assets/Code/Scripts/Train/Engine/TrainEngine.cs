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
        [SerializeField] private float tangentialForceCost = 10.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float throttleSmoothing;

        private const string ThrottleKey = "Throttle";
        private const string PowerDrawKey = "PowerDraw";
        private const string CurrentKey = "Current";

        private DriverNetwork driverNetwork;
        private Locomotive locomotive;

        private float powerDraw;
        private float throttleActual;

        public float ThrottleInput => driverNetwork.GetValue(ThrottleKey);

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

            if (locomotive.Gear == 0) return;

            var fwdSpeed = locomotive.GetForwardSpeed();
            var oldPowerDraw = powerDraw;
            var connected = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f;
            
            if (connected)
            {
                ApplyDriveForce(fwdSpeed);
            }
            else
            {
                powerDraw = 0.0f;
            }
            
            UpdatePowerDraw(oldPowerDraw);
        }

        private void UpdatePowerDraw(float oldPowerDraw)
        {
            powerDraw = (powerDraw + oldPowerDraw) / 2.0f;
            driverNetwork.SetValue(PowerDrawKey, powerDraw);
        }

        private void ApplyDriveForce(float fwdSpeed)
        {
            var maxSpeed = maxSpeedKmpH / 3.6f;
            var acceleration = (maxSpeed - Mathf.Abs(fwdSpeed)) * this.acceleration * throttleActual * locomotive.Gear;

            locomotive.Rigidbody.AddForce(locomotive.DriverDirection * acceleration * locomotive.ReferenceWeight);
            powerDraw = throttleActual * maxPowerConsumption + Mathf.Abs(Vector3.Dot(locomotive.TangentialForce, transform.right)) * tangentialForceCost;
        }

        public float CalculatePowerDraw() => powerDraw;
    }
}