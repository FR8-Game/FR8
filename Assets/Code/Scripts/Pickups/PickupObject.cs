using FR8.Interactions.Drivers;
using FR8.Player;
using UnityEngine;

namespace FR8.Pickups
{
    [RequireComponent(typeof(Rigidbody))]
    public class PickupObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName;
        [SerializeField] private Vector3 holdTranslation;
        [SerializeField] private Vector3 holdRotation;

        private new Rigidbody rigidbody;
        private PlayerGroundedAvatar target;

        public bool CanInteract => !target;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string DisplayValue => target ? "Drop" : "Pickup";
        
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            if (!target) return;
            transform.position = target.CameraTarget.TransformPoint(holdTranslation);
            transform.rotation = target.CameraTarget.rotation * Quaternion.Euler(holdRotation);
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
