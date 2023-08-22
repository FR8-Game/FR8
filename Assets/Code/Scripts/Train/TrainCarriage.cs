using FR8.Train.Track;
using UnityEngine;

namespace FR8.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class TrainCarriage : MonoBehaviour
    {
        [Space]
        [SerializeField] protected float drag = 12.0f;
        [SerializeField] protected float referenceWeight;
        [SerializeField] protected float cornerLean = 0.6f;

        [Space]
        [SerializeField] protected float retentionSpring = 2500.0f;
        [SerializeField] protected float retentionDamping = 50.0f;
        [SerializeField] protected float retentionTorqueConstant = 0.2f;

        [Space]
        [SerializeField] protected TrackSegment segment;

        public TrackSegment Segment
        {
            get => segment;
            set => segment = value;
        }

        public Rigidbody Rigidbody { get; private set; }
        public Vector3 DriverDirection { get; private set; }
        public float ReferenceWeight => referenceWeight;
        public Vector3 TangentialForce { get; private set; }

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
            Rigidbody.AddForce(DriverDirection * force);
        }

        private void ApplyCorrectiveForce()
        {
            // Calculate pose of front wheel assembly
            var t = segment.GetClosestPoint(Rigidbody.position);
            var trackPosition = segment.SamplePoint(t);
            var trackNormal = segment.SampleTangent(t);
            trackNormal.Normalize();

            var alignmentDot = Vector3.Dot(trackNormal, transform.forward);
            if (alignmentDot < 0.0f) trackNormal = -trackNormal;

            DriverDirection = trackNormal;
            
            ApplyCorrectiveForce(trackPosition, trackNormal);
            
            var orientation = CalculateOrientation(trackNormal);
            ApplyCorrectiveTorque(orientation);
        }

        private void ApplyCorrectiveForce(Vector3 position, Vector3 direction)
        {
            // Calculate alignment delta as a force
            var force = (position - Rigidbody.position) * retentionSpring;

            // Calculate damping force
            var normalVelocity = Rigidbody.velocity;
            force -= normalVelocity * retentionDamping;

            // Apply Force
            force -= direction * Vector3.Dot(direction, force);
            Rigidbody.AddForce(force, ForceMode.Acceleration);
            TangentialForce = transform.InverseTransformVector(force);
        }

        private Quaternion CalculateOrientation(Vector3 direction)
        {
            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            var lean = Mathf.Asin(Vector3.Dot(transform.right, direction)) * cornerLean * fwdSpeed;

            var rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(Vector3.forward * lean);
            return rotation;
        }

        private void ApplyCorrectiveTorque(Quaternion rotation)
        {
            var deltaRotation = CalculateDeltaRotation(rotation);

            var torque = (deltaRotation * retentionSpring - Rigidbody.angularVelocity * retentionDamping) * retentionTorqueConstant;
            Rigidbody.AddTorque(torque, ForceMode.Acceleration);
        }

        private Vector3 CalculateDeltaRotation(Quaternion rotation)
        {
            (rotation * Quaternion.Inverse(Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
            if (angle > 180.0f) angle -= 360.0f;
            axis.Normalize();
            if (!float.IsFinite(axis.x) || !float.IsFinite(axis.y) || !float.IsFinite(axis.z)) axis = Vector3.zero;

            var deltaRotation = axis * angle * Mathf.Rad2Deg;
            return deltaRotation;
        }

        public float GetForwardSpeed() => Vector3.Dot(DriverDirection, Rigidbody.velocity);
        
        private void OnValidate()
        {
            Configure();
        }

        protected static float ToMps(float kmph) => kmph / 3.6f;
        protected static float ToKmpH(float mps) => mps * 3.6f;
    }
}