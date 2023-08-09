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

        private PlayerGroundedAvatar target;

        private Vector3 lastTargetPosition;
        private Quaternion lastTargetRotation;

        public Rigidbody Rigidbody { get; private set; }
        public virtual bool CanInteract => !Held;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string DisplayValue => target ? "Drop" : "Pickup";
        public bool Held => target;

        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new System.NotImplementedException();

        public void Nudge(int direction)
        {
        }

        public void BeginDrag(Ray ray)
        {
        }

        public void ContinueDrag(Ray ray)
        {
        }

        public Vector3 HoldTranslation => (pickupPose ? pickupPose.holdTranslation : Vector3.zero) + additionalTranslation;
        public Quaternion HoldRotation => Quaternion.Euler(pickupPose ? pickupPose.holdRotation : Vector3.zero) * Quaternion.Euler(additionalRotation);

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void FixedUpdate()
        {
            if (!target) return;

            var targetPosition = GetTargetPosition();
            var targetRotation = GetTargetRotation();

            var targetVelocity = (targetPosition - lastTargetPosition) / Time.deltaTime;
            var (angle, axis) = Utility.Quaternion.ToAngleAxis(targetRotation * Quaternion.Inverse(lastTargetRotation));

            var targetAngularVelocity = axis * angle / Time.deltaTime * Mathf.Deg2Rad;

            var force = (targetPosition - Rigidbody.position) * spring + (targetVelocity - Rigidbody.velocity) * damping;
            Rigidbody.AddForce(force - Physics.gravity, ForceMode.Acceleration);

            (angle, axis) = Utility.Quaternion.ToAngleAxis(targetRotation * Quaternion.Inverse(Rigidbody.rotation));

            angle *= Mathf.Deg2Rad;
            if (angle == 0.0f)
            {
                axis = Vector3.up;
                angle = 0.0f;
            }

            var torque = (axis * angle * spring + (targetAngularVelocity - Rigidbody.angularVelocity) * damping) * torqueScaling;
            Rigidbody.AddTorque(torque, ForceMode.Acceleration);

            lastTargetPosition = targetPosition;
            lastTargetRotation = targetRotation;
        }

        private Vector3 GetTargetPosition() => target.CameraTarget.TransformPoint(HoldTranslation);
        private Quaternion GetTargetRotation() => target.CameraTarget.rotation * HoldRotation;

        public virtual PickupObject Pickup(PlayerGroundedAvatar target)
        {
            if (this.target) return null;
            if (!target) return null;

            this.target = target;
            Rigidbody.detectCollisions = false;

            lastTargetPosition = GetTargetPosition();
            lastTargetRotation = GetTargetRotation();

            return this;
        }

        public virtual PickupObject Drop()
        {
            if (!target) return null;

            Rigidbody.detectCollisions = true;

            target = null;
            return null;
        }
    }
}