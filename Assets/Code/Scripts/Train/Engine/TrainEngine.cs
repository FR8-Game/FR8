using FR8.Interactions.Drivers;
using FR8.Train.Electrics;
using UnityEngine;

namespace FR8.Train.Engine
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrainDrive))]
    public sealed class TrainEngine : TrainElectrics
    {
        [SerializeField] private float maxSpeed;
        [SerializeField] private float acceleration;
        [SerializeField] private float maxSpeedPowerConsumptionMegawatts = 75.0f;

        private float velocity;
        private float force;
        private DriverGroup throttleDriver;
        private TrainDrive train;

        public float Throttle => Connected ? (throttleDriver ? throttleDriver.Value : 0.0f) : 0.0f;

        private void Awake()
        {
            train = GetComponent<TrainDrive>();
            
            var findDriver = DriverGroup.Find(gameObject);

            throttleDriver = findDriver("Throttle");
        }

        private void FixedUpdate()
        {
            if (train.Gear == 0) return;
            
            var fwdSpeed = train.GetForwardSpeed();
            var maxSpeed = this.maxSpeed / 3.6f;
            var force = (maxSpeed * train.Gear - fwdSpeed) / maxSpeed * acceleration * Throttle;
            train.Rigidbody.AddForce(train.DriverDirection * force, ForceMode.Acceleration);
        }

        public override float CalculatePowerConsumptionMegawatts() => -Throttle * maxSpeedPowerConsumptionMegawatts;
    }
}
