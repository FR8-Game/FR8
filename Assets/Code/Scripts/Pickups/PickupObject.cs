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

        private new Rigidbody rigidbody;
        private PlayerGroundedAvatar target;

        public bool CanInteract => !target;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string DisplayValue => target ? "Drop" : "Pickup";
        public Vector3 HoldTranslation => (pickupPose ? pickupPose.holdTranslation : Vector3.zero) + additionalTranslation;
        public Quaternion HoldRotation => Quaternion.Euler(pickupPose ? pickupPose.holdRotation : Vector3.zero) * Quaternion.Euler(additionalRotation);
        
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            if (!target) return;
            transform.position = target.CameraTarget.TransformPoint(HoldTranslation);
            transform.rotation = target.CameraTarget.rotation * HoldRotation;
        }

        public PickupObject Pickup(PlayerGroundedAvatar target)
        {
            if (this.target) return null;
            if (!target) return null;
            
            this.target = target;
            rigidbody.detectCollisions = false;
            rigidbody.isKinematic = true;
            return this;
        }

        public PickupObject Drop()
        {
            if (!target) return null;
            
            rigidbody.detectCollisions = true;
            rigidbody.isKinematic = false;
            rigidbody.velocity = target.Rigidbody.velocity;
            
            target = null;
            return null;
        }
    }
}
