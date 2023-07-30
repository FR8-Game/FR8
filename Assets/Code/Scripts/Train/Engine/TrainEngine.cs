using System;
using FR8.Interactions.Drivers;
using FR8.Train.Electrics;
using UnityEngine;
using UnityEngine.Serialization;

namespace FR8.Train.Engine
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrainDrive))]
    public sealed class TrainEngine : TrainElectrics
    {
        [SerializeField] private float mass;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float acceleration;
        [SerializeField] private float drag;
        [SerializeField] private float maxSpeedPowerConsumptionMegawatts = 75.0f;

        private float position;
        private float velocity;
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
            CalculateForcesAndIntegrate();
            TransferPower();
        }

        private void TransferPower()
        {
            var power = position * mass;
            var trainPower = train.GetForwardSpeed() * train.Rigidbody.mass;
            
            var deltaPower = power - trainPower;
            deltaPower = Mathf.Min(deltaPower, power);
            
            power -= deltaPower;
            position = power / mass;

            train.Rigidbody.AddForce(train.DriverDirection * deltaPower);
        }

        private void CalculateForcesAndIntegrate()
        {
            var maxPower = maxSpeed / 3.6f * train.Rigidbody.mass / mass;
            
            var force = 0.0f;
            
            force += Throttle * (maxPower - position) * acceleration;

            force -= velocity * drag;

            position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
        }

        public override float CalculatePowerConsumptionMegawatts() => -Throttle * maxSpeedPowerConsumptionMegawatts;
    }
}
