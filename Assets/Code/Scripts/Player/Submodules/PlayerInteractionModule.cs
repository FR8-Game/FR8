using System;
using FR8.Drivers;
using FR8.Rendering;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace FR8.Player.Submodules
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerAvatar))]
    public class PlayerInteractionModule : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 2.5f;
        [SerializeField] private TMP_Text readoutText;

        private PlayerController controller;
        private new Camera camera;

        private int nudge;
        private bool press;
        private bool dragging;
        private float dragDistance;

        private IInteractable lookingAt;

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

            if (newLookingAt != lookingAt)
            {
                if (lookingAt != null) SelectionOutlinePass.RemovePersistant(lookingAt.gameObject);
                if (newLookingAt != null) SelectionOutlinePass.RenderPersistant(newLookingAt.gameObject);
            }

            lookingAt = newLookingAt;

            if (lookingAt == null)
            {
                readoutText.text = null;
                press = false;
                nudge = 0;
                dragging = false;
                return;
            }

            var alpha = $"<alpha={(lookingAt.CanInteract ? "#FF" : "#80")}>";
            readoutText.text = $"{alpha}{lookingAt.DisplayName}\n<size=66%>{lookingAt.DisplayValue}";

            if (lookingAt.CanInteract)
            {
                if (controller.Drag)
                {
                    if (dragging) lookingAt.ContinueDrag(ray);
                    else lookingAt.BeginDrag(ray);
                }

                if (nudge != 0)
                {
                    lookingAt.Nudge(nudge);
                    nudge = 0;
                }

                if (press)
                {
                    lookingAt.Press();
                    press = false;
                }
            }

            nudge = 0;
            press = false;
            dragging = controller.Drag;
        }

        private IInteractable GetLookingAt()
        {
            var ray = GetLookingRay();
            if (!Physics.Raycast(ray, out var hit, interactionDistance)) return null;

            return hit.collider .GetComponentInParent<IInteractable>();
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