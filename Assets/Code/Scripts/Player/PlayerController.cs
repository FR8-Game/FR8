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
        private InputActionReference flyAction;
        private InputActionReference crouchAction;

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

        public bool JumpTriggered => jumpInput.action?.WasPressedThisFrame() ?? false;
        public bool Jump => jumpInput.Switch();

        public Vector2 LookFrameDelta
        {
            get
            {
                var delta = Vector2.zero;

                delta += lookAction.action?.ReadValue<Vector2>() * controllerSensitivity * Time.deltaTime ?? Vector2.zero;
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    delta += mouse.delta.ReadValue() * mouseSensitivity;
                }

                return delta;
            }
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