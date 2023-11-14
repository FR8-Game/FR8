using System.Collections.Generic;
using FR8.Runtime.Interactions.Drivers.Submodules;
using FR8.Runtime.Rendering.Passes;
using FR8.Runtime.References;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace FR8.Runtime.Player
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
        private IEnumerable<Renderer> visuals;
        
        public bool CanInteract => true;
        public string DisplayName => name;
        public string DisplayValue => "";
        public bool OverrideInteractDistance => false;
        public float InteractDistance { get; }
        public IInteractable.InteractionType Type => IInteractable.InteractionType.Press;

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
            visuals = GetComponentsInChildren<Renderer>();
        }

        public void Nudge(int direction) { }

        public void BeginInteract(GameObject interactingObject)
        {
            if (interactingObject.TryGetComponent(out PlayerAvatar avatar))
            {
                SoundReference.PlayerSit.PlayOneShot();
                avatar.SetMount(this);
            }
        }

        public void ContinueInteract(GameObject interactingObject) { }
        
        public void Highlight(bool highlight)
        {
            if (highlight) SelectionOutlinePass.Add(visuals);
            else SelectionOutlinePass.Remove(visuals);
        }
    }
}
