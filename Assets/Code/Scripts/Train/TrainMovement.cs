using FR8.Drivers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace FR8.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrainMovement : MonoBehaviour
    {
        [SerializeField] private float acceleration = 1.0f;
        [SerializeField] private float drag;
        [SerializeField] private float referenceWeight;

        [Space]
        [SerializeField] private float brakeConstant = 0.05f;
        [SerializeField] private float brakeMin = 0.5f;
        [SerializeField] private float brakeMax = 4.0f;

        [Space]
        [SerializeField] private SplineContainer track;
        [SerializeField] private int currentSplineIndex;

        [Space]
        [SerializeField] private DriverGroup throttleDriver;
        [SerializeField] private DriverGroup brakeDriver;
        [SerializeField] private DriverGroup gearDriver;

        public Rigidbody Rigidbody { get; private set; }

        public float Throttle => throttleDriver ? throttleDriver.Value : 0.0f;
        public float Brake => brakeDriver ? brakeDriver.Value : 0.0f;
        public int Gear => gearDriver ? Mathf.RoundToInt(gearDriver.Value) : 0;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            ApplyThrottle();
            ApplyBrake();
            ApplyDrag();
            ApplyCorrectiveForce();
        }


        private void ApplyThrottle()
        {
            Rigidbody.AddForce(transform.forward * Gear * Throttle * acceleration * referenceWeight);
        }

        private void ApplyDrag()
        {
            var fwdSpeed = Vector3.Dot(transform.forward, Rigidbody.velocity);
            var drag = -fwdSpeed * Mathf.Abs(fwdSpeed) * this.drag;

            var velocityChange = drag * referenceWeight / Rigidbody.mass * Time.deltaTime;
            if (-velocityChange > fwdSpeed) velocityChange = -fwdSpeed;

            Rigidbody.AddForce(transform.forward * velocityChange, ForceMode.VelocityChange);
        }

        private void ApplyBrake()
        {
            var fwdSpeed = Vector3.Dot(transform.forward, Rigidbody.velocity);
            var force = -CalculateBrakeForce(fwdSpeed) * Brake * Mathf.Sign(fwdSpeed);

            var velocityChange = force * referenceWeight / Rigidbody.mass * Time.deltaTime;
            if (-velocityChange > fwdSpeed) velocityChange = -fwdSpeed;

            Rigidbody.AddForce(transform.forward * velocityChange, ForceMode.VelocityChange);
        }
        
        public float CalculateBrakeForce(float speed) => Mathf.Clamp(speed * speed * brakeConstant, brakeMin, brakeMax);

        private void ApplyCorrectiveForce()
        {
            var spline = track.Splines[currentSplineIndex];
            SplineUtility.GetNearestPoint(spline, Rigidbody.position, out var nearest, out var t);
            nearest = track.transform.TransformPoint(nearest);

            var force = ((Vector3)nearest - Rigidbody.position) / Time.deltaTime;

            Debug.DrawLine(Rigidbody.position, nearest);

            var normalVelocity = Rigidbody.velocity;
            normalVelocity -= transform.forward * Vector3.Dot(transform.forward, Rigidbody.velocity);
            force -= normalVelocity;

            Rigidbody.AddForce(force, ForceMode.VelocityChange);

            var tangent = spline.EvaluateTangent(t);
            Rigidbody.MoveRotation(Quaternion.LookRotation(tangent, Vector3.up));
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            Rigidbody = GetComponent<Rigidbody>();
            referenceWeight = Rigidbody.mass;

            brakeConstant = Mathf.Max(0.0f, brakeConstant);
            brakeMax = Mathf.Max(0.0f, brakeMax);
        }
    }
}