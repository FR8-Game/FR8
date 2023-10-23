using System.Collections.Generic;
using FR8Runtime.Interactions.Drivers.Submodules;
using FR8Runtime.Player;
using FR8Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8Runtime.Pickups
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class PickupObject : MonoBehaviour, IInteractable
    {
        public const int DefaultLayer = 0;
        public const int PickupLayer = 8;

        public string displayName;
        public PickupPose pickupPose;
        public Vector3 additionalTranslation;
        public Vector3 additionalRotation;

        public float spring = 300.0f;
        public float damping = 18.0f;
        public float torqueScaling = 1.0f;

        private PlayerAvatar target;

        private Vector3 lastTargetPosition;
        private Quaternion lastTargetRotation;
        private Renderer[] visuals;

        public Rigidbody Rigidbody { get; private set; }
        public virtual bool CanInteract => true;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public virtual string DisplayValue => target ? "Drop" : "Pickup";
        public bool Held => target;
        public IInteractable.InteractionType Type => IInteractable.InteractionType.Press;

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
        
        public void Highlight(bool highlight)
        {
            if (highlight) SelectionOutlinePass.Add(visuals);
            else SelectionOutlinePass.Remove(visuals);
        }

        public Vector3 HoldTranslation => (pickupPose ? pickupPose.holdTranslation : Vector3.zero) + additionalTranslation;
        public Quaternion HoldRotation => Quaternion.Euler(pickupPose ? pickupPose.holdRotation : Vector3.zero) * Quaternion.Euler(additionalRotation);

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            visuals = GetComponentsInChildren<Renderer>();
            Rigidbody.gameObject.layer = PickupLayer;

            transform.SetParent(null);
        }

        protected virtual void FixedUpdate()
        {
            if (!target) return;

            var targetPosition = GetTargetPosition();
            var targetRotation = GetTargetRotation();

            var targetVelocity = (targetPosition - lastTargetPosition) / Time.deltaTime;
            var (angle, axis) = CodeUtility.QuaternionUtility.ToAngleAxis(targetRotation * Quaternion.Inverse(lastTargetRotation));

            var targetAngularVelocity = axis * angle / Time.deltaTime * Mathf.Deg2Rad;

            var force = (targetPosition - Rigidbody.position) * spring + (targetVelocity - Rigidbody.velocity) * damping;
            Rigidbody.AddForce(force - Physics.gravity, ForceMode.Acceleration);

            (angle, axis) = CodeUtility.QuaternionUtility.ToAngleAxis(targetRotation * Quaternion.Inverse(Rigidbody.rotation));

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

        private Vector3 GetTargetPosition() => target.Head.TransformPoint(HoldTranslation);
        private Quaternion GetTargetRotation() => target.Head.rotation * HoldRotation;
    }
}