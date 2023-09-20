using System;
using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Train.Electrics;
using FR8Runtime.Train.Engine;
using FR8Runtime.Train.Splines;
using UnityEngine;
using UnityEngine.Serialization;

namespace FR8Runtime.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrainEngine), typeof(TrainElectricsController), typeof(TrainGasTurbine))]
    [RequireComponent(typeof(DriverNetwork), typeof(TrainMonitor), typeof(LocomotiveAudio))]
    public class Locomotive : TrainCarriage
    {
        [FormerlySerializedAs("brakeConstant")]
        [SerializeField] private float dynamicBrakeConstant = 0.5f;

        [SerializeField] private float staticBrakeConstant = 25.0f;

        [Space]
        [Header("Testing")]
        [SerializeField] private float initialVelocity;

        private const string BrakeKey = "Brake";
        private const string GearKey = "Gear";
        private const string SpeedometerKey = "Speed";

        private DriverNetwork driverNetwork;

        private float engineVelocity;
        private float enginePower;

        public float Brake => driverNetwork.GetValue(BrakeKey);
        public int Gear => Mathf.RoundToInt(driverNetwork.GetValue(GearKey));

        protected override void Configure()
        {
            base.Configure();

            dynamicBrakeConstant = Mathf.Max(0.0f, dynamicBrakeConstant);
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        protected override void Start()
        {
            base.Start();
            
#if UNITY_EDITOR
            var t = segment.GetClosestPoint(Rigidbody.position);
            var dir = segment.SampleTangent(t);
            Rigidbody.AddForce(dir * initialVelocity / 3.6f, ForceMode.VelocityChange);
#endif
        }

        protected override void FixedUpdate()
        {
            ApplyBrake();
            base.FixedUpdate();

            UpdateDriverGroups();
        }

        private void ApplyBrake()
        {
            var fwdSpeed = GetForwardSpeed();
            var constant = Mathf.Abs(fwdSpeed) > 1.0f ? dynamicBrakeConstant : staticBrakeConstant;

            var force = constant * Brake * -fwdSpeed;

            var velocityChange = force * referenceWeight / Rigidbody.mass * Time.deltaTime;
            if (Mathf.Abs(velocityChange) > Mathf.Abs(fwdSpeed)) velocityChange = -fwdSpeed;

            Rigidbody.AddForce(DriverDirection * velocityChange, ForceMode.VelocityChange);
        }

        public void UpdateDriverGroups()
        {
            var fwdSpeed = Mathf.Abs(ToKmpH(GetForwardSpeed()));
            driverNetwork.SetValue(SpeedometerKey, fwdSpeed);
        }
    }
}