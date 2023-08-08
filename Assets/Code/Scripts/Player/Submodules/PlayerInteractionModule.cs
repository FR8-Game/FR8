using System;
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
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerAvatar))]
    public class PlayerInteractionModule : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 2.5f;
        [SerializeField] private TMP_Text readoutText;
        [SerializeField] private Utility.DampedSpring transition;

        private PlayerController controller;
        private new Camera camera;

        private int nudge;
        private bool press;
        private bool dragging;
        private float dragDistance;

        private IInteractable lookingAt;
        private PickupObject heldObject;

        public void Awake()
        {
            controller = transform.parent.GetComponent<PlayerController>();
            camera = Camera.main;
        }

        public void Update()
        {
            if (controller.Press) press = true;
            if (controller.Nudge != 0) nudge = controller.Nudge;
        }

        private void OnDisable()
        {
            if ((Object)lookingAt) SelectionOutlinePass.RemovePersistant(lookingAt.gameObject);
        }

        public void FixedUpdate()
        {
            var ray = GetLookingRay();
            var newLookingAt = dragging ? lookingAt : GetLookingAt();
            if (heldObject) newLookingAt = heldObject;

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
                press = false;
                nudge = 0;
                dragging = false;
                return;
            }

            var alpha = $"<alpha={(lookingAt.CanInteract ? "#FF" : "#80")}>";
            readoutText.text = $"{alpha}{lookingAt.DisplayName}\n<size=66%>{lookingAt.DisplayValue}";

            if (heldObject)
            {
                if (press)
                {
                    heldObject = heldObject.Drop();
                    press = false;
                }
            }
            else if (lookingAt.CanInteract)
            {
                switch (lookingAt)
                {
                    case PickupObject pickup:
                        ProcessPickup(pickup);
                        break;
                    default:
                        ProcessInteractable(lookingAt, ray);
                        break;
                }
            }

            nudge = 0;
            press = false;
            dragging = controller.Drag;
        }

        private void ProcessInteractable(IInteractable driver, Ray ray)
        {
            if (controller.Drag)
            {
                if (dragging) driver.ContinueDrag(ray);
                else driver.BeginDrag(ray);
            }

            if (nudge != 0)
            {
                driver.Nudge(nudge);
                nudge = 0;
            }
        }

        private void ProcessPickup(PickupObject pickup)
        {
            if (!press) return;

            heldObject = pickup.Pickup(controller.CurrentAvatar as PlayerGroundedAvatar);
                
            press = false;
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

            return camera.ScreenPointToRay(position);
        }
    }
}