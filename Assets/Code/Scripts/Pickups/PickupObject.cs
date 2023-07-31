using FR8.Interactions.Drivers;
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
        [SerializeField] private float error = 1800;
        [SerializeField] private float torqueScaling = 1.0f;

        private new Rigidbody rigidbody;
        private PlayerGroundedAvatar target;
        
        private Vector3 translationError;
        private Vector3 rotationError;

        // private Vector3 holdPosition, holdVelocity;
        // private Quaternion holdRotation;
        // private Vector3 holdAngularVelocity;
        
        public bool CanInteract => !target;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string DisplayValue => target ? "Drop" : "Pickup";
        public Vector3 HoldTranslation => (pickupPose ? pickupPose.holdTranslation : Vector3.zero) + additionalTranslation;
        public Quaternion HoldRotation => Quaternion.Euler(pickupPose ? pickupPose.holdRotation : Vector3.zero) * Quaternion.Euler(additionalRotation);
        
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!target) return;
            
            var targetPosition = target.CameraTarget.TransformPoint(HoldTranslation);
            var targetRotation = target.CameraTarget.rotation * HoldRotation;

            var force = (targetPosition - rigidbody.position) * spring - rigidbody.velocity * damping + translationError * error;
            rigidbody.AddForce(force - Physics.gravity, ForceMode.Acceleration);

            var deltaRotation = (targetRotation * Quaternion.Inverse(rigidbody.rotation));
            if (deltaRotation.w < 0.0f) deltaRotation = Quaternion.Inverse(deltaRotation);
            deltaRotation.ToAngleAxis(out var angle, out var axis);
            if (Mathf.Abs(angle) > 180.0f) angle = 360.0f - angle;
            
            angle *= Mathf.Deg2Rad;
            if (angle == 0.0f)
            {
                axis = Vector3.up;
                angle = 0.0f;
            }
            
            var torque = (axis * angle * spring - rigidbody.angularVelocity * damping + rotationError * error) * torqueScaling;
            rigidbody.AddTorque(torque, ForceMode.Acceleration);

            translationError += (targetPosition - rigidbody.position) * Time.deltaTime;
            rotationError += axis * angle * Time.deltaTime;
        }

        public PickupObject Pickup(PlayerGroundedAvatar target)
        {
            if (this.target) return null;
            if (!target) return null;
            
            this.target = target;
            rigidbody.detectCollisions = false;
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
