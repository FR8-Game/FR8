using System.Collections.Generic;
using FR8Runtime.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class PlayerMount : MonoBehaviour, IInteractable
    {
        [SerializeField] private Vector3 offset;
        [SerializeField] private bool limitRotation;
        [SerializeField] private Vector2 rotationLimit;
        [SerializeField] private Vector2 rotationLimitOffset;
        [SerializeField] private Vector3 dismountOffset;

        private Rigidbody body;
        
        public bool CanInteract => true;
        public string DisplayName => name;
        public string DisplayValue => "";
        public bool OverrideInteractDistance => false;
        public float InteractDistance { get; }
        public IEnumerable<Renderer> Visuals { get; private set; }

        public Vector3 Position => transform.TransformPoint(offset);
        public Quaternion Rotation => transform.rotation;
        public Vector3 Velocity => body ? body.velocity : Vector3.zero;
        public Vector3 DismountPosition => transform.TransformPoint(dismountOffset);

        public Quaternion ConstrainRotation(Quaternion q)
        {
            if (!limitRotation) return q.normalized;
            
            var rotation = new Vector2(q.eulerAngles.y, q.eulerAngles.x);

            rotation -= rotationLimitOffset;

            rotation.x = Mathf.Clamp(rotation.x, -rotationLimit.x, rotationLimit.x);
            rotation.y = Mathf.Clamp(rotation.y, -rotationLimit.y, rotationLimit.y);
            
            rotation += rotationLimitOffset;
            return Quaternion.Euler(rotation.y, rotation.x, 0.0f);
        }
        
        private void Awake()
        {
            body = GetComponentInParent<Rigidbody>();
            Visuals = GetComponentsInChildren<Renderer>();
        }

        public void Nudge(int direction) { }

        public void BeginInteract(GameObject interactingObject)
        {
            if (interactingObject.TryGetComponent(out PlayerAvatar avatar))
            {
                avatar.SetMount(this);
            }
        }

        public void ContinueInteract(GameObject interactingObject) { }
    }
}
