using System;
using FR8Runtime.Interactions.Drivers.Submodules;
using FR8Runtime.Pickups;
using FR8Runtime.Rendering.Passes;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace FR8Runtime.Player.Submodules
{
    [Serializable]
    public class PlayerInteractionManager
    {
        [SerializeField] private float interactionDistance = 2.5f;

        private int nudge;
        private bool dragging;
        private IInteractable lastHighlightedObject;
        
        public IInteractable HighlightedObject { get; private set; }

        public PickupObject HeldObject { get; private set; }
        public PlayerAvatar Avatar { get; private set; }

        public event Action<PickupObject> PickupEvent;
        public event Action<PickupObject> DropEvent;
        
        public void Init(PlayerAvatar avatar)
        {
            Avatar = avatar;

            avatar.UpdateEvent += Update;
            avatar.FixedUpdateEvent += FixedUpdate;
            avatar.DisableEvent += OnDisable;
        }

        public void OnDisable()
        {
            if ((Object)lastHighlightedObject) lastHighlightedObject.Highlight(false);
        }
        
        public void Update()
        {
            if (Avatar.input.Nudge != 0) nudge = Avatar.input.Nudge;
        }

        public void FixedUpdate()
        {
            UpdateHighlightedObject();

            if ((Object)HighlightedObject)
            {
                ProcessInteractable();
                UpdateInputFlags();
            }
            else
            {
                ResetInputFlags();
            }
        }

        private void UpdateInputFlags()
        {
            nudge = 0;
            dragging = Avatar.input.Drag;
        }

        private void ResetInputFlags()
        {
            nudge = 0;
            dragging = false;
        }
        
        private void UpdateHighlightedObject()
        {
            HighlightedObject = GetHighlightedObject();

            if (lastHighlightedObject == HighlightedObject) return;
            
            if ((Object)lastHighlightedObject) lastHighlightedObject.Highlight(false);
            if ((Object)HighlightedObject) HighlightedObject.Highlight(true);
            
            lastHighlightedObject = HighlightedObject;
        }

        private IInteractable GetHighlightedObject()
        {
            if (dragging) return HighlightedObject;

            var lookingAt = GetLookingAt();
            if ((Object)lookingAt) return lookingAt;

            if (HeldObject) return HeldObject;

            return null;
        }

        private void ProcessInteractable()
        {
            if (!HighlightedObject.CanInteract) return;

            if (Avatar.input.Drag)
            {
                if (dragging) HighlightedObject.ContinueInteract(Avatar.gameObject);
                else HighlightedObject.BeginInteract(Avatar.gameObject);
            }

            if (nudge != 0)
            {
                HighlightedObject.Nudge(nudge);
                nudge = 0;
            }
        }

        public PlayerInteractionManager Pickup(PickupObject pickup)
        {
            if (HeldObject) return null;

            HeldObject = pickup.Pickup(Avatar);
            
            PickupEvent?.Invoke(HeldObject);
            return this;
        }

        public PlayerInteractionManager Drop()
        {
            DropEvent?.Invoke(HeldObject);
            HeldObject = HeldObject.Drop(Avatar);
            return null;
        }

        private IInteractable GetLookingAt()
        {
            var ray = GetLookingRay();
            if (!Physics.Raycast(ray, out var hit)) return null;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (!(Object)interactable) return null;
            if (!interactable.isActiveAndEnabled) return null;

            var distance = interactable.OverrideInteractDistance ? interactable.InteractDistance : interactionDistance;
            return (hit.distance < distance) ? interactable : null;
        }

        private Ray GetLookingRay()
        {
            var position = new Vector2(Screen.width, Screen.height) / 2.0f;
            switch (Cursor.lockState)
            {
                case CursorLockMode.None:
                case CursorLockMode.Confined:
                    var mouse = Mouse.current;
                    if (mouse == null) break;
                    position = mouse.position.ReadValue();
                    break;
                case CursorLockMode.Locked:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Avatar.cameraController.Camera.ScreenPointToRay(position);
        }
    }
}