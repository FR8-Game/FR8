using System;
using System.IO.Pipes;
using FR8.Interactions.Drivers.Submodules;
using FR8.Pickups;
using FR8.Rendering.Passes;
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

        private IInteractable lookingAt;

        public PickupObject HeldObject { get; private set; }
        public PlayerAvatar Avatar { get; private set; }

        public void Init(PlayerAvatar avatar)
        {
            Avatar = avatar;

            avatar.UpdateEvent += Update;
            avatar.FixedUpdateEvent += FixedUpdate;
            avatar.DisabledEvent += OnDisable;
        }

        public void Update()
        {
            if (Avatar.input.Press) { }

            if (Avatar.input.Nudge != 0) nudge = Avatar.input.Nudge;
        }

        public void OnDisable()
        {
            if ((Object)lookingAt) SelectionOutlinePass.RemovePersistant(lookingAt.gameObject);
        }

        public void FixedUpdate()
        {
            var ray = GetLookingRay();
            var newLookingAt = dragging ? lookingAt : GetLookingAt();
            if (HeldObject && !(Object)newLookingAt) newLookingAt = HeldObject;

            if (newLookingAt != lookingAt)
            {
                transition.currentPosition = 0.0f;
                transition.velocity = 0.0f;

                if ((Object)lookingAt) SelectionOutlinePass.RemovePersistant(lookingAt.gameObject);
                if ((Object)newLookingAt) SelectionOutlinePass.RenderPersistant(newLookingAt.gameObject);
            }

            lookingAt = newLookingAt;
            transition.Target(lookingAt != null ? 1.0f : 0.0f).Iterate(Time.deltaTime);
            var animatePosition = Mathf.Max(0.0f, transition.currentPosition);
            readoutText.transform.localScale = Vector3.one * animatePosition;
            readoutText.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (1.0f - animatePosition) * 20.0f);

            if (lookingAt == null)
            {
                nudge = 0;
                dragging = false;
                return;
            }

            var alpha = $"<alpha={(lookingAt.CanInteract ? "#FF" : "#80")}>";
            readoutText.text = $"{alpha}{lookingAt.DisplayName}\n<size=66%>{lookingAt.DisplayValue}";

            if (lookingAt.CanInteract)
            {
                ProcessInteractable(lookingAt, ray);
            }

            nudge = 0;
            dragging = Avatar.input.Drag;
        }

        private void ProcessInteractable(IInteractable driver, Ray ray)
        {
            if (Avatar.input.Drag)
            {
                if (dragging) driver.ContinueInteract(Avatar.gameObject);
                else driver.BeginInteract(Avatar.gameObject);
            }

            if (nudge != 0)
            {
                driver.Nudge(nudge);
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