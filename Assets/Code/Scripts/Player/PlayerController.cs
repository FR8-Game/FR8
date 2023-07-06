using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(PlayerGroundedMovement))]
    [RequireComponent(typeof(PlayerMovementController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputMap;
        
        [Header("Camera")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float mouseSensitivity = 0.3f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float controllerSensitivity = 0.4f;
        
        [Space]
        [SerializeField] private float cameraFieldOfView = 70.0f;

        private new Camera camera;

        private InputActionReference moveInput;
        private InputActionReference jumpInput;
        private InputActionReference lookAction;
        private InputActionReference flyAction;
        private InputActionReference crouchAction;
        
        public Rigidbody Rigidbody { get; private set; }
        public Vector3 CameraOffset { get; set; }
        public Quaternion GlobalCameraOrientation { get; set; } = Quaternion.identity;

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

            // Configure Object
            Configure();

            // Setup input
            inputMap.Enable();
            moveInput = bind("Move");
            jumpInput = bind("Jump");
            lookAction = bind("Look");
            crouchAction = bind("Crouch");
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnValidate()
        {
            Configure();
        }

        public void Configure()
        {
            camera = Camera.main;
            if (camera)
            {
                GlobalCameraOrientation = camera.transform.rotation;
                camera.fieldOfView = cameraFieldOfView;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 1000.0f;
            }
        }

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        #endregion

        #region Loop
        
        private void FixedUpdate()
        {
            MoveCamera();
        }
        
        private void LateUpdate()
        {
            MoveCamera();
        }

        #endregion

        #region Managment

        #endregion

        #region Camera

        public void MoveCamera()
        {
            camera.transform.position = transform.position + transform.rotation * CameraOffset;
            camera.transform.rotation = GlobalCameraOrientation;
        }

        #endregion
    }
}