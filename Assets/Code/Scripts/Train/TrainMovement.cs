using System.Text.RegularExpressions;
using FR8.Track;
using UnityEngine;
using UnityEngine.Splines;

namespace FR8.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class TrainMovement : MonoBehaviour
    {
        [Space]
        [SerializeField] protected float drag;
        [SerializeField] protected float referenceWeight;
        [SerializeField] protected float cornerLean = 0.6f;

        [Space]
        [SerializeField] protected float retentionSpring = 100.0f;
        [SerializeField] protected float retentionDamping = 10.0f;
        [SerializeField] protected float retentionTorqueConstant = 4.0f;

        [Space]
        [SerializeField] protected TrackSegment track;

        [Space]
        [SerializeField] protected int curveSampleIterations = 15;

        private Transform frontWheelAssembly;
        private Transform rearWheelAssembly;

        public Rigidbody Rigidbody { get; private set; }
        public Vector3 DriverDirection => frontWheelAssembly ? frontWheelAssembly.transform.forward : transform.forward;

        private void Awake()
        {
            Configure();
        }

        protected virtual void Configure()
        {
            Rigidbody = GetComponent<Rigidbody>();
            if (!Application.isPlaying)
            {
                referenceWeight = Rigidbody.mass;
            }

            var wheelGroup = Utility.Hierarchy.FindOrCreate(transform, new Regex(@".*wheel.*", RegexOptions.Compiled | RegexOptions.IgnoreCase), "Wheels");
            frontWheelAssembly = Utility.Hierarchy.FindOrCreate(wheelGroup, new Regex(@".*front.*", RegexOptions.Compiled | RegexOptions.IgnoreCase), "Front Wheel Assembly");
            rearWheelAssembly = Utility.Hierarchy.FindOrCreate(wheelGroup, new Regex(@".*rear.*", RegexOptions.Compiled | RegexOptions.IgnoreCase), "Rear Wheel Assembly");
        }

        protected virtual void FixedUpdate()
        {
            ApplyDrag();
            ApplyCorrectiveForce();
        }

        private void ApplyDrag()
        {
            var fwdSpeed = GetForwardSpeed();
            var drag = -fwdSpeed * Mathf.Abs(fwdSpeed) * this.drag;

            var force = drag * referenceWeight / Rigidbody.mass;
            Rigidbody.AddForce(DriverDirection * force, ForceMode.Acceleration);
        }

        private void ApplyCorrectiveForce()
        {
            // Calculate pose of front wheel assembly
            var oldFrontPosition = frontWheelAssembly.position;
            var (frontPosition, frontTangent) = pointOnSpline(oldFrontPosition);
            frontTangent.Normalize();
            frontWheelAssembly.transform.rotation = Quaternion.LookRotation(frontTangent, Vector3.up);

            // Calculate alignment delta as a force
            var force = (frontPosition - oldFrontPosition) * retentionSpring;

            // Calculate damping force
            var normalVelocity = Rigidbody.velocity;
            normalVelocity -= frontTangent * Vector3.Dot(frontTangent, Rigidbody.velocity);
            force -= normalVelocity * retentionDamping;

            // Apply Force
            Rigidbody.AddForce(force, ForceMode.Acceleration);

            // Lineup rear wheel assembly as best as possible.
            var rearPosition = rearWheelAssembly.position;
            var rearTangent = DriverDirection;
            var distance = (frontPosition - rearPosition).magnitude;
            for (var i = 0; i < curveSampleIterations; i++)
            {
                (rearPosition, rearTangent) = pointOnSpline(rearPosition);
                rearPosition = (rearPosition - frontPosition).normalized * distance + frontPosition;
            }
            
            // Rotate Rear Wheel Assembly to align with spline.
            rearWheelAssembly.rotation = Quaternion.LookRotation(rearTangent, Vector3.up);

            // Calculate orientation of train
            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            var lean = Mathf.Asin(Vector3.Dot(transform.right, frontTangent)) * cornerLean * fwdSpeed;
            
            var direction = (frontPosition - rearPosition).normalized;
            var rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(Vector3.forward * lean);
            
            // Calculate torque to resolve rotation
            (rotation * Quaternion.Inverse(Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
            if (angle > 180.0f) angle -= 360.0f; 
            axis.Normalize();
            if (!float.IsFinite(axis.x) || !float.IsFinite(axis.y) || !float.IsFinite(axis.z)) axis = Vector3.zero;
            
            var torque = (axis * angle * Mathf.Deg2Rad * retentionSpring - Rigidbody.angularVelocity * retentionDamping) * retentionTorqueConstant;
            Rigidbody.AddTorque(torque, ForceMode.Acceleration);

            // Local Functions
            (Vector3, Vector3) pointOnSpline(Vector3 position)
            {
                var t = track.ClosestPoint(position);
                var pointOnSpline = track. 
                SplineUtility.GetNearestPoint(spline, position, out var pointOnSpline, out var t);
                pointOnSpline = track.transform.TransformPoint(pointOnSpline);

                var tangent = track.transform.TransformDirection(spline.EvaluateTangent(t)).normalized;
                return (pointOnSpline, tangent);
            }
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

        protected static float ToMps(float kmph) => kmph / 3.6f;
        protected static float ToKmpH(float mps) => mps * 3.6f;
    }
}