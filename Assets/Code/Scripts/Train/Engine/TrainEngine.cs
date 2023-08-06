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
        [SerializeField] private float powerFromSpeed = 1.0f;
        [SerializeField] private float powerFromDifference = 1.0f;
        [SerializeField] private float amperesScaling = 0.01f;
        [SerializeField][Range(0.0f, 1.0f)] private float throttleSmoothing;

        private const string ThrottleKey = "Throttle";
        private const string PowerDrawKey = "PowerDraw";
        private const string CurrentKey = "Current";

        private DriverNetwork driverNetwork;
        private Locomotive train;

        private bool connected;
        private float ps;
        private float pd;
        private float powerConsumption;
        private float throttleActual;

        public float ThrottleInput => driverNetwork.Read(ThrottleKey);
        public string FuseGroup => null;

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
            throttleActual += (ThrottleInput - throttleActual) * (1.0f - throttleSmoothing);
            
            if (train.Gear == 0) return;

            var fwdSpeed = train.GetForwardSpeed();

            if (connected)
            {
                var maxSpeed = maxSpeedKmpH / 3.6f;
                var targetSpeed = maxSpeed * throttleActual * train.Gear;

                var acceleration = Mathf.Clamp((targetSpeed - fwdSpeed) / maxSpeed, -1.0f, 1.0f) * accelerationTime;

                train.Rigidbody.AddForce(train.DriverDirection * acceleration * train.ReferenceWeight);

                ps = Mathf.Abs(fwdSpeed) * powerFromSpeed;
                pd = Mathf.Abs(targetSpeed - fwdSpeed) * powerFromDifference;
                
                powerConsumption = ps + pd;
            }
            else
            {
                ps = 0.0f;
                pd = 0.0f;
                powerConsumption = 0.0f;
            }

            driverNetwork.SetValue(PowerDrawKey, powerConsumption / 1000.0f);
            driverNetwork.SetValue(CurrentKey, pd * amperesScaling);
        }

        public void SetConnected(bool connected) => this.connected = connected;

        public float CalculatePowerDraw() => powerConsumption;
    }
}