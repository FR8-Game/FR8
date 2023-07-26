using FR8.Track;
using UnityEngine;

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
        [SerializeField] protected TrackWalker walker;

        [Space]
        [SerializeField] protected int curveSampleIterations = 15;

        public Rigidbody Rigidbody { get; private set; }
        public Vector3 DriverDirection { get; private set; }

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
            Rigidbody.AddForce(DriverDirection * force, ForceMode.Acceleration);
        }

        private void ApplyCorrectiveForce()
        {
            // Calculate pose of front wheel assembly

            var position = walker.Position;
            var direction = walker.Tangent;
            
            direction.Normalize();
            DriverDirection = direction;
            
            // Calculate alignment delta as a force
            var force = (position - Rigidbody.position) * retentionSpring;
            
            // Calculate damping force
            var normalVelocity = Rigidbody.velocity;
            normalVelocity -= direction * Vector3.Dot(direction, Rigidbody.velocity);
            force -= normalVelocity * retentionDamping;
            
            // Apply Force
            Rigidbody.AddForce(force, ForceMode.Acceleration);
            
            // Calculate orientation of train
            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            var lean = Mathf.Asin(Vector3.Dot(transform.right, direction)) * cornerLean * fwdSpeed;
            
            var rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(Vector3.forward * lean);
            
            // Calculate torque to resolve rotation
            (rotation * Quaternion.Inverse(Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
            if (angle > 180.0f) angle -= 360.0f; 
            axis.Normalize();
            if (!float.IsFinite(axis.x) || !float.IsFinite(axis.y) || !float.IsFinite(axis.z)) axis = Vector3.zero;
            
            var torque = (axis * angle * Mathf.Deg2Rad * retentionSpring - Rigidbody.angularVelocity * retentionDamping) * retentionTorqueConstant;
            Rigidbody.AddTorque(torque, ForceMode.Acceleration);
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