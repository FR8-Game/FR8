using FR8.Drivers;
using UnityEngine;

namespace FR8.Train
{
    public class TrainDrive : TrainMovement
    {
        [SerializeField] private float acceleration = 4.0f;
        [SerializeField] private float maxSpeed = 80.0f;
        [SerializeField] private float maxSpeedBlending = 10.0f;
        [SerializeField] private float engineSmoothTime = 1.0f;
        
        [Space]
        [SerializeField] private float brakeConstant = 4.0f;
        
        private DriverGroup throttleDriver;
        private DriverGroup brakeDriver;
        private DriverGroup gearDriver;
        private DriverGroup speedometerDriver;
        
        private float engineVelocity;
        private float enginePower;
        
        public float Throttle => throttleDriver ? throttleDriver.Value : 0.0f;
        public float Brake => brakeDriver ? brakeDriver.Value : 0.0f;
        public int Gear => gearDriver ? Mathf.RoundToInt(gearDriver.Value) : 0;

        protected override void Configure()
        {
            base.Configure();
            
            acceleration = Mathf.Max(0.0f, acceleration);
            brakeConstant = Mathf.Max(0.0f, brakeConstant);
            
            var drivers = GetComponentsInChildren<DriverGroup>();
            foreach (var driver in drivers)
            {
                switch (driver.name)
                {
                    case "Throttle":
                        throttleDriver = driver;
                        break;
                    case "Brake":
                        brakeDriver = driver;
                        break;
                    case "Gear":
                        gearDriver = driver;
                        break;
                    case "Speedometer":
                        speedometerDriver = driver;
                        break;
                }
            }
        }

        protected override void FixedUpdate()
        {
            ApplyThrottle();
            ApplyBrake();
            base.FixedUpdate();
            
            UpdateDriverGroups();
        }

        private void ApplyThrottle()
        {
            enginePower = Mathf.SmoothDamp(enginePower, Throttle, ref engineVelocity, engineSmoothTime);

            var force = DriverDirection * Gear * enginePower * ToMps(acceleration);
            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            var slowdown = Mathf.InverseLerp(ToMps(maxSpeed), ToMps(maxSpeed - maxSpeedBlending), fwdSpeed);
            force *= slowdown;
            
            Rigidbody.AddForce(force * referenceWeight);
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
            var fwdSpeed = Mathf.Abs(ToKmpH(Vector3.Dot(DriverDirection, Rigidbody.velocity)));
            speedometerDriver.SetValue(fwdSpeed);
        }
    }
}
