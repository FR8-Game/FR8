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
        [SerializeField] private float acceleration = 10.0f;
        [SerializeField] private float internalFriction = 1.0f;
        [SerializeField] private float maxSpeedPowerConsumptionMegawatts = 75.0f;
        [SerializeField] private float stallLoad;
        [SerializeField] private float currentLoad;
        [SerializeField] private Vector2 rpmRemap = Vector2.up;

        private float velocity;
        private float force;
        
        private DriverGroup throttleDriver;
        private DriverGroup rpmDriver;
        private Locomotive train;

        public float Throttle => Connected ? (throttleDriver ? throttleDriver.Value : 0.0f) : 0.0f;

        private void Awake()
        {
            train = GetComponent<Locomotive>();
            
            var findDriver = DriverGroup.Find(gameObject);
            throttleDriver = findDriver("Throttle");
            rpmDriver = findDriver("Rpm");
        }

        private void FixedUpdate()
        {
            if (train.Gear == 0) return;
            
            var maxSpeed = this.maxSpeedKmpH / 3.6f;
            var acceleration = this.acceleration * Throttle * train.Gear;

            acceleration -= velocity * internalFriction;
            
            var engineFriction = acceleration / (maxSpeed * maxSpeed);
            var drag = Mathf.Abs(velocity) * velocity * engineFriction;
            acceleration -= drag;

            velocity += acceleration / train.Rigidbody.mass * train.ReferenceWeight * Time.deltaTime;
            ApplyWheelFriction();

            var rpm = Mathf.Lerp(rpmRemap.x, rpmRemap.y, Mathf.InverseLerp(0.0f, maxSpeed, velocity));
            rpmDriver.SetValue(rpm);
        }

        private void ApplyWheelFriction()
        {
            var trainVelocity = train.GetForwardSpeed();

            var targetVelocity = (velocity + trainVelocity) / 2.0f;

            currentLoad = Mathf.Abs(targetVelocity - velocity);
            velocity = targetVelocity;
            if (currentLoad > stallLoad)
            {
                return;
            }
            
            var trainVelocityChange = targetVelocity - trainVelocity;
            train.Rigidbody.AddForce(train.DriverDirection * trainVelocityChange, ForceMode.VelocityChange);
        }

        public override float CalculatePowerConsumptionMegawatts() => -Throttle * maxSpeedPowerConsumptionMegawatts;
    }
}
