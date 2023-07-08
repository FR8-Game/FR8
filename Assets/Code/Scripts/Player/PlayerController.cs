using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputMap;

        [Header("Camera")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float mouseSensitivity = 0.3f;

        [Range(0.0f, 1.0f)]
        [SerializeField] private float controllerSensitivity = 0.4f;

        private InputActionReference moveInput;
        private InputActionReference jumpInput;
        private InputActionReference lookAction;
        private InputActionReference crouchAction;
        private InputActionReference nudgeAction;
        private InputActionReference pressAction;
        private InputActionReference freeCamAction;
        private InputActionReference grabCamAction;

        private PlayerAvatar[] avatars;
        private PlayerAvatar currentAvatar;

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

        public Vector2 GetLookFrameDelta(bool forceMouseDelta)
        {
            var delta = Vector2.zero;

            delta += lookAction.action?.ReadValue<Vector2>() * controllerSensitivity * Time.deltaTime ?? Vector2.zero;
            var mouse = Mouse.current;
            if (mouse != null && (Cursor.lockState == CursorLockMode.Locked || forceMouseDelta))
            {
                delta += mouse.delta.ReadValue() * mouseSensitivity;
            }

            return delta;
        }
        
        #region Initalization

        private void Awake()
        {
            // Local Functions
            InputActionReference bind(string name) => InputActionReference.Create(inputMap.FindAction(name));

            // Setup input
            inputMap.Enable();
            moveInput = bind("Move");
            jumpInput = bind("Jump");
            lookAction = bind("Look");
            crouchAction = bind("Crouch");
            nudgeAction = bind("Nudge");
            pressAction = bind("Press");
            freeCamAction = bind("FreeCam");
            grabCamAction = bind("GrabCam");

            // Configure Avatars
            avatars = new PlayerAvatar[transform.childCount];
            for (var i = 0; i < avatars.Length; i++)
            {
                avatars[i] = transform.GetChild(i).GetComponent<PlayerAvatar>();
                avatars[i].gameObject.SetActive(false);
            }

            SetAvatar(0);
        }

        public void SetAvatar<T>() where T : PlayerAvatar
        {
            for (var i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].GetType() != typeof(T)) continue;
                SetAvatar(i);
                break;
            }
        }

        public void SetAvatar(int index)
        {
            for (var i = 0; i < avatars.Length; i++)
            {
                if (i == index) continue;
                avatars[i].gameObject.SetActive(false);
            }

            if (index < 0 || index >= avatars.Length) return;
            currentAvatar = avatars[index];
            currentAvatar.gameObject.SetActive(true);
        }

        #endregion
    }
}