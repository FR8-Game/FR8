﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8.Player
{
    [System.Serializable]
    public class PlayerInput
    {
        [SerializeField] private InputActionAsset inputMap;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float mouseSensitivity = 0.3f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float controllerSensitivity = 0.4f;

        private Camera mainCamera;
        
        private InputActionReference moveInput;
        private InputActionReference jumpInput;
        private InputActionReference lookAction;
        private InputActionReference sprintAction;
        private InputActionReference crouchAction;
        private InputActionReference nudgeAction;
        private InputActionReference pressAction;
        private InputActionReference freeCamAction;
        private InputActionReference grabCamAction;
        private InputActionReference zoomCamAction;
        private InputActionReference peeAction;


        public Vector3 Move
        {
            get
            {
                var xz = moveInput.action?.ReadValue<Vector2>() ?? Vector2.zero;
                var y = (jumpInput.action?.ReadValue<float>() ?? 0.0f) - (crouchAction.action?.ReadValue<float>() ?? 0.0f);
                return new Vector3(xz.x, y, xz.y);
            }
        }

        public bool JumpTriggered => jumpInput.action?.WasPerformedThisFrame() ?? false;
        public bool Jump => jumpInput.Switch();
        public Vector2 LookFrameDelta => GetLookFrameDelta(false);

        public int Nudge => Mathf.Clamp(Mathf.RoundToInt(nudgeAction.action?.ReadValue<float>() ?? 0.0f), -1, 1);
        public bool Press => pressAction.action?.WasPerformedThisFrame() ?? false;
        public bool Drag => pressAction.Switch();
        public bool FreeCam => freeCamAction.action?.WasPerformedThisFrame() ?? false;
        public bool GrabCam => grabCamAction.Switch();
        public bool ZoomCam => zoomCamAction.Switch();
        public bool Sprint => sprintAction.Switch();
        public bool Pee => peeAction.Switch();

        public Vector2 GetLookFrameDelta(bool forceMouseDelta)
        {
            var fovSensitivity = Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            
            var delta = Vector2.zero;

            delta += (lookAction.action?.ReadValue<Vector2>() * controllerSensitivity * Time.deltaTime ?? Vector2.zero);
            var mouse = Mouse.current;
            if (mouse != null && (Cursor.lockState == CursorLockMode.Locked || forceMouseDelta))
            {
                delta += mouse.delta.ReadValue() * mouseSensitivity;
            }

            return delta * fovSensitivity;
        }

        public void Init()
        {
            // Local Functions
            InputActionReference bind(string name) => InputActionReference.Create(inputMap.FindAction(name));

            // Setup input
            inputMap.Enable();
            moveInput = bind("Move");
            jumpInput = bind("Jump");
            lookAction = bind("Look");
            sprintAction = bind("Sprint");
            crouchAction = bind("Crouch");
            nudgeAction = bind("Nudge");
            pressAction = bind("Press");
            freeCamAction = bind("FreeCam");
            grabCamAction = bind("GrabCam");
            zoomCamAction = bind("ZoomCam");
            peeAction = bind("Pee");
            
            mainCamera = Camera.main;
        }
    }
}