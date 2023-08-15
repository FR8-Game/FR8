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
        public const int DefaultLayer = 0;
        public const int PickupLayer = 8;

        [SerializeField] private string displayName;
        [SerializeField] private PickupPose pickupPose;
        [SerializeField] private Vector3 additionalTranslation;
        [SerializeField] private Vector3 additionalRotation;

        [SerializeField] private float spring = 300.0f;
        [SerializeField] private float damping = 18.0f;
        [SerializeField] private float torqueScaling = 1.0f;

        private PlayerAvatar target;

        private Vector3 lastTargetPosition;
        private Quaternion lastTargetRotation;

        public Rigidbody Rigidbody { get; private set; }
        public virtual bool CanInteract => true;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public virtual string DisplayValue => target ? "Drop" : "Pickup";
        public bool Held => target;

        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new System.NotImplementedException();

        public void Nudge(int direction) { }

        public void BeginInteract(GameObject interactingObject)
        {
            var avatar = interactingObject.GetComponentInParent<PlayerAvatar>();
            if (!avatar) return;
            if (target && avatar != target) return;

            if (Held) target.interactionManager.Drop();
            else avatar.interactionManager.Pickup(this);
        }

        public virtual PickupObject Pickup(PlayerAvatar target)
        {
            this.target = target;
            
            Rigidbody.detectCollisions = false;
            lastTargetPosition = GetTargetPosition();
            lastTargetRotation = GetTargetRotation();

            return this;
        }

        public virtual PickupObject Drop(PlayerAvatar target)
        {
            this.target = null;
            Rigidbody.detectCollisions = true;
            
            return null;
        }

        public void ContinueInteract(GameObject interactingObject) { }

        public Vector3 HoldTranslation => (pickupPose ? pickupPose.holdTranslation : Vector3.zero) + additionalTranslation;
        public Quaternion HoldRotation => Quaternion.Euler(pickupPose ? pickupPose.holdRotation : Vector3.zero) * Quaternion.Euler(additionalRotation);

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.gameObject.layer = PickupLayer;

            transform.SetParent(null);
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
    }
}