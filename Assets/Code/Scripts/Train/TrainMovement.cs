using FR8.Drivers;
using UnityEngine;
using UnityEngine.Splines;

namespace FR8.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrainMovement : MonoBehaviour
    {
        [SerializeField] private float acceleration = 4.0f;
        [SerializeField] private float drag;
        [SerializeField] private float referenceWeight;
        [SerializeField] private float maxSpeed = 80.0f;
        [SerializeField] private float maxSpeedBlending = 10.0f;

        [Space]
        [SerializeField] private float brakeConstant = 4.0f;

        [Space]
        [SerializeField] private SplineContainer track;
        [SerializeField] private int currentSplineIndex;

        [Space]
        [SerializeField] private int curveSampleIterations = 5;

        private DriverGroup throttleDriver;
        private DriverGroup brakeDriver;
        private DriverGroup gearDriver;
        private DriverGroup speedometerDriver;

        private Transform frontWheelAssembly;
        private Transform rearWheelAssembly;

        public Vector3 DriverDirection => frontWheelAssembly ? frontWheelAssembly.transform.forward : transform.forward;
        public Rigidbody Rigidbody { get; private set; }

        public float Throttle => throttleDriver ? throttleDriver.Value : 0.0f;
        public float Brake => brakeDriver ? brakeDriver.Value : 0.0f;
        public int Gear => gearDriver ? Mathf.RoundToInt(gearDriver.Value) : 0;

        private void Awake()
        {
            Configure();
        }

        private void Configure()
        {
            Rigidbody = GetComponent<Rigidbody>();
            if (!Application.isPlaying)
            {
                referenceWeight = Rigidbody.mass;
            }

            acceleration = Mathf.Max(0.0f, acceleration);
            brakeConstant = Mathf.Max(0.0f, brakeConstant);

            var wheelGroup = Utility.Hierarchy.FindOrCreate(transform, "Wheels");
            frontWheelAssembly = Utility.Hierarchy.FindOrCreate(wheelGroup, "Front Wheel Assembly");
            rearWheelAssembly = Utility.Hierarchy.FindOrCreate(wheelGroup, "Rear Wheel Assembly");

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

        private void FixedUpdate()
        {
            ApplyThrottle();
            ApplyBrake();
            ApplyDrag();
            ApplyCorrectiveForce();
            UpdateDriverGroups();
        }

        private void ApplyThrottle()
        {
            var force = DriverDirection * Gear * Throttle * ToMps(acceleration);
            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            var slowdown = Mathf.InverseLerp(ToMps(maxSpeed), ToMps(maxSpeed - maxSpeedBlending), fwdSpeed);
            force *= slowdown;
            
            Rigidbody.AddForce(force * referenceWeight);
        }

        private void ApplyDrag()
        {
            var fwdSpeed = GetForwardSpeed();
            var drag = -fwdSpeed * Mathf.Abs(fwdSpeed) * this.drag;

            var force = drag * referenceWeight / Rigidbody.mass;
            Rigidbody.AddForce(DriverDirection * force, ForceMode.Acceleration);
        }

        private void ApplyBrake()
        {
            var fwdSpeed = GetForwardSpeed();
            var force = ToMps(brakeConstant) * Brake * -Mathf.Sign(fwdSpeed);

            var velocityChange = force * referenceWeight / Rigidbody.mass * Time.deltaTime;
            if (Mathf.Abs(velocityChange) > Mathf.Abs(fwdSpeed)) velocityChange = -fwdSpeed;

            Rigidbody.AddForce(DriverDirection * velocityChange, ForceMode.VelocityChange);
        }

        private void ApplyCorrectiveForce()
        {
            // Calculate and Apply Force for Front Wheel Assembly
            var oldFrontPosition = frontWheelAssembly.position;

            var spline = track.Splines[currentSplineIndex];
            var (frontPosition, frontTangent) = pointOnSpline(oldFrontPosition);
            frontWheelAssembly.transform.rotation = Quaternion.LookRotation(frontTangent, Vector3.up);

            var force = (frontPosition - oldFrontPosition) / Time.deltaTime;

            var normalVelocity = Rigidbody.velocity;
            normalVelocity -= DriverDirection * Vector3.Dot(DriverDirection, Rigidbody.velocity);
            force -= normalVelocity;

            Rigidbody.AddForce(force, ForceMode.VelocityChange);

            // Lineup rear wheel assembly as best as possible.
            var rearPosition = rearWheelAssembly.position;
            var rearTangent = DriverDirection;
            var distance = (frontPosition - rearPosition).magnitude;
            for (var i = 0; i < curveSampleIterations; i++)
            {
                (rearPosition, rearTangent) = pointOnSpline(rearPosition);
                rearPosition = (rearPosition - frontPosition).normalized * distance + frontPosition;
            }

            rearWheelAssembly.rotation = Quaternion.LookRotation(rearTangent, Vector3.up);

            var direction = (frontPosition - rearPosition).normalized;
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            (rotation * Quaternion.Inverse(Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
            
            if (angle > 180.0f) angle -= 360.0f; 
            axis.Normalize();
            if (!float.IsFinite(axis.x) || !float.IsFinite(axis.y) || !float.IsFinite(axis.z)) axis = Vector3.zero;
            
            var torque = axis * angle * Mathf.Deg2Rad / Time.deltaTime - Rigidbody.angularVelocity;
            Rigidbody.AddTorque(torque, ForceMode.VelocityChange);

            // Local Functions
            (Vector3, Vector3) pointOnSpline(Vector3 position)
            {
                SplineUtility.GetNearestPoint(spline, position, out var pointOnSpline, out var t);
                pointOnSpline = track.transform.TransformPoint(pointOnSpline);

                var tangent = track.transform.TransformDirection(spline.EvaluateTangent(t)).normalized;
                return (pointOnSpline, tangent);
            }
        }

        public void UpdateDriverGroups()
        {
            var fwdSpeed = Mathf.Abs(ToKmpH(Vector3.Dot(DriverDirection, Rigidbody.velocity)));
            speedometerDriver.SetValue(fwdSpeed);
        }

        public float GetForwardSpeed() => Vector3.Dot(DriverDirection, Rigidbody.velocity);
        
        private void OnValidate()
        {
            Configure();
        }

        private void OnDrawGizmos()
        {
            if (!frontWheelAssembly || !rearWheelAssembly) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(frontWheelAssembly.position, 1.0f);
            Gizmos.DrawWireSphere(rearWheelAssembly.position, 1.0f);
            Gizmos.DrawLine(rearWheelAssembly.position, frontWheelAssembly.position);
        }

        private static float ToMps(float kmph) => kmph / 3.6f;
        private static float ToKmpH(float mps) => mps * 3.6f;
    }
}