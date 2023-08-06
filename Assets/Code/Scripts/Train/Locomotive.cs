using FR8.Interactions.Drivers;
using UnityEngine;

namespace FR8.Train
{
    public class Locomotive : TrainCarriage
    {
        [SerializeField] private float brakeConstant = 4.0f;

        private DriverGroup brakeDriver;
        private DriverGroup gearDriver;
        private DriverGroup speedometerDriver;
        
        private float engineVelocity;
        private float enginePower;
        
        public float Brake => brakeDriver ? brakeDriver.Value : 0.0f;
        public int Gear => gearDriver ? Mathf.RoundToInt(gearDriver.Value) : 0;

        protected override void Configure()
        {
            base.Configure();
            
            brakeConstant = Mathf.Max(0.0f, brakeConstant);

            var findDriver = DriverGroup.Find(gameObject);
            
            brakeDriver = findDriver("Brake");
            gearDriver = findDriver("Gear");
            speedometerDriver = findDriver("Speedometer");
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
            speedometerDriver.SetValue(fwdSpeed);
        }
    }
}
