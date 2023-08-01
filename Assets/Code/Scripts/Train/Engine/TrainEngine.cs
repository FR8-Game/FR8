using FR8.Interactions.Drivers;
using FR8.Train.Electrics;
using UnityEngine;

namespace FR8.Train.Engine
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Locomotive))]
    public sealed class TrainEngine : TrainElectrics
    {
        [SerializeField] private float maxSpeedKmpH = 120.0f;
        [SerializeField] private float accelerationTime = 5.0f;
        [SerializeField] private float voltageScaling = 1500.0f;
        [SerializeField] private float currentScaling = 4.0f;
        [SerializeField] private float trackMagnetDistance = 2.0f;

        private DriverGroup throttleDriver;
        private DriverGroup powerDrawDriver;
        private DriverGroup currentDriver;
        private Locomotive train;

        private float voltage;
        private float current;
        private float powerConsumption;

        public float Throttle => Connected ? (throttleDriver ? throttleDriver.Value : 0.0f) : 0.0f;

        private void Awake()
        {
            train = GetComponent<Locomotive>();
            
            var findDriver = DriverGroup.Find(gameObject);
            throttleDriver = findDriver("Throttle");
            powerDrawDriver = findDriver("Power Draw");
            currentDriver = findDriver("Current");
        }

        private void FixedUpdate()
        {
            if (train.Gear == 0) return;

            var fwdSpeed = train.GetForwardSpeed();
            var maxSpeed = maxSpeedKmpH / 3.6f;
            var targetSpeed = maxSpeed * Throttle * train.Gear;
            var acceleration = Mathf.Clamp((targetSpeed - fwdSpeed) / maxSpeed, -1.0f, 1.0f) * accelerationTime;
            
            train.Rigidbody.AddForce(train.DriverDirection * acceleration * train.ReferenceWeight);

            current = Mathf.Abs(fwdSpeed) / trackMagnetDistance * currentScaling;
            voltage = Mathf.Abs(acceleration) * voltageScaling;
            powerConsumption = current * voltage;

            powerDrawDriver.SetValue(powerConsumption / 1000.0f);
            currentDriver.SetValue(current);
        }

        public override float CalculatePowerConsumptionMegawatts() => -powerConsumption;
    }
}
