using FR8.Runtime.Save;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8.Runtime.Player.Submodules
{
    [System.Serializable]
    public class PlayerInput
    {
        [SerializeField] private InputActionAsset inputMap;

        private float MouseSensitivity => SaveManager.SettingsSave.GetOrLoad().mouseSensitivity;
        private Vector2 ControllerSensitivity => new()
        {
            x = SaveManager.SettingsSave.GetOrLoad().gamepadSensitivityX,
            y = SaveManager.SettingsSave.GetOrLoad().gamepadSensitivityY,
        };

        private Camera mainCamera;
        private PlayerAvatar avatar;

        public InputActionReference moveInput;
        public InputActionReference jumpInput;
        public InputActionReference lookAction;
        public InputActionReference sprintAction;
        public InputActionReference crouchAction;
        public InputActionReference nudgeAction;
        public InputActionReference pressAction;
        public InputActionReference freeCamAction;
        public InputActionReference grabCamAction;
        public InputActionReference zoomCamAction;
        public InputActionReference peeAction;
        public InputActionReference flyAction;
        public InputActionReference[] hotbarActions;
        public InputActionReference hotbarNext;
        public InputActionReference hotbarLast;

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
        public Vector2 LookFrameDelta => GetLookFrameDelta();

        public int Nudge => (nudgeAction.action?.WasPerformedThisFrame() ?? false) ? Mathf.Clamp(Mathf.RoundToInt(nudgeAction.action?.ReadValue<float>() ?? 0.0f), -1, 1) : 0;
        public bool Press => pressAction.action?.WasPerformedThisFrame() ?? false;
        public bool Drag => pressAction.State();
        public bool FreeCam => freeCamAction.action?.WasPerformedThisFrame() ?? false;
        public bool GrabCam => grabCamAction.State();
        public bool ZoomCam => zoomCamAction.State();
        public bool Sprint => sprintAction.State();
        public bool Pee => peeAction.State();
        public bool Fly => flyAction.action?.WasPerformedThisFrame() ?? false;

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

        public int MoveHotbar => (hotbarNext.action?.WasPerformedThisFrame() ?? false ? 1 : 0) - (hotbarLast.action?.WasPerformedThisFrame() ?? false ? 1 : 0);

        public bool Crouch => crouchAction.State();

        public void EnableInput(bool state)
        {
            if (state) inputMap.Enable();
            else inputMap.Disable();
        }
        
        public Vector2 GetLookFrameDelta()
        {
            var fovSensitivity = GetFovSensitivity();
            var delta = Vector2.zero;

            delta += GetAdditiveLookInput();
            delta += GetMouseLookInput();

            return delta * fovSensitivity;
        }

        private Vector2 GetMouseLookInput()
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                return mouse.delta.ReadValue() * MouseSensitivity;
            }

            return Vector2.zero;
        }

        private Vector2 GetAdditiveLookInput()
        {
            return lookAction.action?.ReadValue<Vector2>() * ControllerSensitivity * 1000.0f * Time.deltaTime ?? Vector2.zero;
        }

        private float GetFovSensitivity() => Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;

            inputMap = Object.Instantiate(inputMap);
            
            // Local Functions
            InputActionReference bind(string name) => InputActionReference.Create(inputMap.FindAction(name));

            // Setup input
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
            flyAction = bind("Fly");

            hotbarActions = new InputActionReference[PlayerInventory.HotbarSize];
            for (var i = 0; i < hotbarActions.Length; i++)
            {
                hotbarActions[i] = bind($"Hotbar.{i + 1}");
            }

            hotbarNext = bind("Hotbar.Next");
            hotbarLast = bind("Hotbar.Last");

            // Set Camera
            mainCamera = Camera.main;

            avatar.vitality.IsAliveChangedEvent += IsAliveChanged;

            EnableInput(true);
        }

        private void IsAliveChanged()
        {
            if (avatar.vitality.IsAlive) inputMap.Enable();
            else inputMap.Disable();
        }
    }
}