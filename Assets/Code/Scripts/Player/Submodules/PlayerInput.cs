using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8Runtime.Player.Submodules
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
        private PlayerAvatar avatar;
        
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
        private InputActionReference[] hotbarActions;
        
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
        public bool Jump => jumpInput.State();
        public Vector2 LookFrameDelta => GetLookFrameDelta(false);

        public int Nudge => Mathf.Clamp(Mathf.RoundToInt(nudgeAction.action?.ReadValue<float>() ?? 0.0f), -1, 1);
        public bool Press => pressAction.action?.WasPerformedThisFrame() ?? false;
        public bool Drag => pressAction.State();
        public bool FreeCam => freeCamAction.action?.WasPerformedThisFrame() ?? false;
        public bool GrabCam => grabCamAction.State();
        public bool ZoomCam => zoomCamAction.State();
        public bool Sprint => sprintAction.State();
        public bool Pee => peeAction.State();
        
        public int SwitchHotbar
        {
            get
            {
                for (var i = 0; i < hotbarActions.Length; i++)
                {
                    var action = hotbarActions[i];
                    if (action.action?.WasPerformedThisFrame() ?? false) return i;
                }
                return -1;
            }
        }

        public bool Crouch => crouchAction.State();

        public Vector2 GetLookFrameDelta(bool forceMouseDelta)
        {
            var fovSensitivity = GetFovSensitivity();
            var delta = Vector2.zero;
            
            delta += GetAdditiveLookInput();
            delta += GetMouseLookInput(forceMouseDelta);

            return delta * fovSensitivity;
        }

        private Vector2 GetMouseLookInput(bool forceMouseDelta)
        {
            var mouse = Mouse.current;
            if (mouse != null && (Cursor.lockState == CursorLockMode.Locked || forceMouseDelta))
            {
                return mouse.delta.ReadValue() * mouseSensitivity;
            }
            return Vector2.zero;
        }

        private Vector2 GetAdditiveLookInput()
        {
            return lookAction.action?.ReadValue<Vector2>() * controllerSensitivity * Time.deltaTime ?? Vector2.zero;
        }

        private float GetFovSensitivity() => Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;
            
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

            hotbarActions = new InputActionReference[PlayerInventory.HotbarSize];
            for (var i = 0; i < hotbarActions.Length; i++)
            {
                hotbarActions[i] = bind($"Hotbar.{i + 1}");
            }
            
            // Set Camera
            mainCamera = Camera.main;

            avatar.vitality.IsAliveChangedEvent += IsAliveChanged;
        }

        private void IsAliveChanged()
        {
            if (avatar.vitality.IsAlive) inputMap.Enable();
            else inputMap.Disable();
        }
    }
}