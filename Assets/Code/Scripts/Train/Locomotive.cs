using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Train.Electrics;
using FR8Runtime.Train.Engine;
using UnityEngine;

namespace FR8Runtime.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrainEngine), typeof(TrainElectricsController), typeof(TrainGasTurbine))]
    [RequireComponent(typeof(DriverNetwork), typeof(TrainMonitor), typeof(LocomotiveAudio))]
    public class Locomotive : TrainCarriage
    {
        [SerializeField] private float brakeConstant = 4.0f;

        private const string BrakeKey = "Brake";
        private const string GearKey = "Gear";
        private const string SpeedometerKey = "Speedometer";

        private DriverNetwork driverNetwork;
        
        private float engineVelocity;
        private float enginePower;

        public float Brake => driverNetwork.GetValue(BrakeKey);
        public int Gear => Mathf.RoundToInt(driverNetwork.GetValue(GearKey));

        protected override void Configure()
        {
            base.Configure();
            
            brakeConstant = Mathf.Max(0.0f, brakeConstant);
            driverNetwork = GetComponentInParent<DriverNetwork>();
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
            var force = ToMps(brakeConstant) * Brake * -Mathf.Sign(fwdSpeed);

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
