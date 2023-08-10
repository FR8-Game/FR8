using FR8.Interactions.Drivers;
using FR8.Train.Electrics;
using UnityEngine;

namespace FR8.Train.Engine
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

        private bool connected;
        private float powerDraw;
        private float throttleActual;

        public float ThrottleInput => driverNetwork.Read(ThrottleKey);
        public string FuseGroup => null;

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
            
            if (connected)
            {
                var maxSpeed = maxSpeedKmpH / 3.6f;
                var acceleration = (maxSpeed - Mathf.Abs(fwdSpeed)) * this.acceleration * throttleActual * locomotive.Gear;

                locomotive.Rigidbody.AddForce(locomotive.DriverDirection * acceleration * locomotive.ReferenceWeight);

                powerDraw = throttleActual * maxPowerConsumption + Mathf.Abs(Vector3.Dot(locomotive.TangentialForce, transform.right)) * tangentialForceCost;
            }
            else
            {
                powerDraw = 0.0f;
            }
            powerDraw = (powerDraw + oldPowerDraw) / 2.0f;

            driverNetwork.SetValue(PowerDrawKey, powerDraw);
        }

        public void SetConnected(bool connected) => this.connected = connected;

        public float CalculatePowerDraw() => powerDraw;
    }
}