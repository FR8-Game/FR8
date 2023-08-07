using FR8.Interactions.Drivers.Submodules;
using FR8.Player;
using UnityEngine;

namespace FR8.Pickups
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class PickupObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName;
        [SerializeField] private PickupPose pickupPose;
        [SerializeField] private Vector3 additionalTranslation;
        [SerializeField] private Vector3 additionalRotation;

        [SerializeField] private float spring = 300.0f;
        [SerializeField] private float damping = 18.0f;
        [SerializeField] private float torqueScaling = 1.0f;

        private new Rigidbody rigidbody;
        private PlayerGroundedAvatar target;
        
        private Vector3 lastTargetPosition;
        private Quaternion lastTargetRotation;

        // private Vector3 holdPosition, holdVelocity;
        // private Quaternion holdRotation;
        // private Vector3 holdAngularVelocity;
        
        public bool CanInteract => !target;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string DisplayValue => target ? "Drop" : "Pickup";
        
        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new System.NotImplementedException();

        public void Nudge(int direction) { }
        public void BeginDrag(Ray ray) { }
        public void ContinueDrag(Ray ray) { }

        public Vector3 HoldTranslation => (pickupPose ? pickupPose.holdTranslation : Vector3.zero) + additionalTranslation;
        public Quaternion HoldRotation => Quaternion.Euler(pickupPose ? pickupPose.holdRotation : Vector3.zero) * Quaternion.Euler(additionalRotation);
        
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!target) return;
            
            var targetPosition = GetTargetPosition();
            var targetRotation = GetTargetRotation();

            var targetVelocity = (targetPosition - lastTargetPosition) / Time.deltaTime;
            (targetRotation * Quaternion.Inverse(lastTargetRotation)).ToAngleAxis(out var angle, out var axis);
            var targetAngularVelocity = axis * angle / Time.deltaTime * Mathf.Deg2Rad;

            var force = (targetPosition - rigidbody.position) * spring + (targetVelocity - rigidbody.velocity) * damping;
            rigidbody.AddForce(force - Physics.gravity, ForceMode.Acceleration);

            var deltaRotation = (targetRotation * Quaternion.Inverse(rigidbody.rotation));
            if (deltaRotation.w < 0.0f) deltaRotation = Quaternion.Inverse(deltaRotation);
            deltaRotation.ToAngleAxis(out angle, out axis);
            if (Mathf.Abs(angle) > 180.0f) angle = 360.0f - angle;
            
            angle *= Mathf.Deg2Rad;
            if (angle == 0.0f)
            {
                axis = Vector3.up;
                angle = 0.0f;
            }
            
            var torque = (axis * angle * spring + (targetAngularVelocity - rigidbody.angularVelocity) * damping) * torqueScaling;
            rigidbody.AddTorque(torque, ForceMode.Acceleration);

            lastTargetPosition = targetPosition;
            lastTargetRotation = targetRotation;
        }

        private Vector3 GetTargetPosition() => target.CameraTarget.TransformPoint(HoldTranslation);
        private Quaternion GetTargetRotation() => target.CameraTarget.rotation * HoldRotation;
        
        public PickupObject Pickup(PlayerGroundedAvatar target)
        {
            if (this.target) return null;
            if (!target) return null;
            
            this.target = target;
            rigidbody.detectCollisions = false;

            lastTargetPosition = GetTargetPosition();
            lastTargetRotation = GetTargetRotation();
            
            return this;
        }

        public PickupObject Drop()
        {
            if (!target) return null;
            
            rigidbody.detectCollisions = true;
            
            target = null;
            return null;
        }
    }
}
