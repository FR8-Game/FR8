using System;
using FR8.Interactions.Drivers.Submodules;
using FR8.Pickups;
using FR8.Rendering.Passes;
using FR8Runtime.Rendering.Passes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;
using Object = UnityEngine.Object;

namespace FR8.Player.Submodules
{
    [System.Serializable]
    public class PlayerInteractionManager
    {
        [SerializeField] private float interactionDistance = 2.5f;
        [SerializeField] private TMP_Text readoutText;
        [SerializeField] private Utility.DampedSpring transition;

        private int nudge;
        private bool dragging;

        private IInteractable highlightedObject;

        public PickupObject HeldObject { get; private set; }
        public PlayerAvatar Avatar { get; private set; }

        public void Init(PlayerAvatar avatar)
        {
            Avatar = avatar;

            avatar.UpdateEvent += Update;
            avatar.FixedUpdateEvent += FixedUpdate;
        }

        public void Update()
        {
            if ((Object)highlightedObject) SelectionOutlinePass.Add(highlightedObject.gameObject);

            if (Avatar.input.Nudge != 0) nudge = Avatar.input.Nudge;
        }

        public void FixedUpdate()
        {
            UpdateHighlightedObject();

            if ((Object)highlightedObject)
            {
                ProcessInteractable();
                UpdateInputFlags();
            }
            else
            {
                ResetInputFlags();
            }

            AnimateUI();
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

        private void AnimateUI()
        {
            var hasHighlightedObject = (bool)(Object)highlightedObject;

            transition.Target(hasHighlightedObject ? 1.0f : 0.0f).Iterate(Time.deltaTime);
            var animatePosition = Mathf.Max(0.0f, transition.currentPosition);

            readoutText.transform.localScale = Vector3.one * animatePosition;
            readoutText.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (1.0f - animatePosition) * 20.0f);

            if (!hasHighlightedObject) return;

            var alpha = $"<alpha={(highlightedObject.CanInteract ? "#FF" : "#80")}>";
            readoutText.text = $"{alpha}{highlightedObject.DisplayName}\n<size=66%>{highlightedObject.DisplayValue}";
        }

        private void UpdateHighlightedObject()
        {
            var newHighlightedObject = GetHighlightedObject();

            if (newHighlightedObject != highlightedObject)
            {
                transition.currentPosition = 0.0f;
                transition.velocity = 0.0f;
            }

            highlightedObject = newHighlightedObject;
        }

        private IInteractable GetHighlightedObject()
        {
            if (dragging) return highlightedObject;

            var lookingAt = GetLookingAt();
            if ((Object)lookingAt) return lookingAt;

            if (HeldObject) return HeldObject;

            return null;
        }

        private void ProcessInteractable()
        {
            if (!highlightedObject.CanInteract) return;

            if (Avatar.input.Drag)
            {
                if (dragging) highlightedObject.ContinueInteract(Avatar.gameObject);
                else highlightedObject.BeginInteract(Avatar.gameObject);
            }

            if (nudge != 0)
            {
                highlightedObject.Nudge(nudge);
                nudge = 0;
            }
        }

        public PlayerInteractionManager Pickup(PickupObject pickup)
        {
            if (HeldObject) return null;

            HeldObject = pickup.Pickup(Avatar);
            return this;
        }

        public PlayerInteractionManager Drop()
        {
            HeldObject = HeldObject.Drop(Avatar);
            return null;
        }

        private IInteractable GetLookingAt()
        {
            var ray = GetLookingRay();
            if (!Physics.Raycast(ray, out var hit)) return null;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (!(Object)interactable) return null;

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