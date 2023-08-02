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
        [SerializeField] private float accelerationTime = 5.0f;
        [SerializeField] private float voltageScaling = 1500.0f;
        [SerializeField] private float currentScaling = 4.0f;
        [SerializeField] private float trackMagnetDistance = 2.0f;

        private const string ThrottleKey = "Throttle";
        private const string PowerDrawKey = "PowerDraw";
        private const string CurrentKey = "Current";

        private DriverNetwork driverNetwork;
        private Locomotive train;

        private bool connected;
        private float voltage;
        private float current;
        private float powerConsumption;

        private float lastFwdSpeed;

        public float Throttle => driverNetwork.Read(ThrottleKey);

        private void Awake()
        {
            train = GetComponent<Locomotive>();
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        private void Start()
        {
            driverNetwork.SetValue(PowerDrawKey, 0.0f);
            driverNetwork.SetValue(CurrentKey, 0.0f);
        }

        private void FixedUpdate()
        {
            if (train.Gear == 0) return;

            var fwdSpeed = train.GetForwardSpeed();

            if (connected)
            {
                var maxSpeed = maxSpeedKmpH / 3.6f;
                var targetSpeed = maxSpeed * Throttle * train.Gear;

                var acceleration = Mathf.Clamp((targetSpeed - fwdSpeed) / maxSpeed, -1.0f, 1.0f) * accelerationTime;

                train.Rigidbody.AddForce(train.DriverDirection * acceleration * train.ReferenceWeight);

                var resistance = Mathf.Abs(fwdSpeed - lastFwdSpeed) / Time.deltaTime;

                current = Mathf.Abs(fwdSpeed) / trackMagnetDistance * currentScaling;
                voltage = Mathf.Abs(acceleration + resistance) * voltageScaling;
                powerConsumption = current * voltage;
            }
            else
            {
                current = 0.0f;
                voltage = 0.0f;
                powerConsumption = 0.0f;
            }

            driverNetwork.SetValue(PowerDrawKey, powerConsumption / 1000.0f);
            driverNetwork.SetValue(CurrentKey, current);
            lastFwdSpeed = fwdSpeed;
        }

        public void SetConnected(bool connected) => this.connected = connected;

        public float CalculatePowerDraw() => powerConsumption;
    }
}