using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(PlayerGroundedMovement), typeof(PlayerNoClip))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputAsset;

        [Header("Camera")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float mouseSensitivity = 0.3f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float controllerSensitivity = 0.4f;

        [Space]
        [SerializeField] private float cameraFieldOfView = 70.0f;
        [SerializeField] private float cameraOffset = -0.1f;
        [SerializeField] private float playerHeight = 1.7f;

        private new Camera camera;

        private Vector2 cameraRotation;
        private new Rigidbody rigidbody;

        private InputActionReference moveInput;
        private InputActionReference jumpInput;
        private InputActionReference lookAction;
        private InputActionReference flyAction;
        private InputActionReference crouchAction;

        private PlayerGroundedMovement playerGroundedMovement;
        private PlayerNoClip playerNoClip;

        #region Initalization

        private void Awake()
        {
            // Local Functions
            InputActionReference bind(string name) => InputActionReference.Create(inputAsset.FindAction(name));

            // Configure Object
            Configure();

            // Setup input
            inputAsset.Enable();
            moveInput = bind("Move");
            jumpInput = bind("Jump");
            lookAction = bind("Look");
            flyAction = bind("Fly");
            crouchAction = bind("Crouch");
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;

            playerGroundedMovement.enabled = true;
            playerNoClip.enabled = false;
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
            cameraRotation = new Vector2(transform.eulerAngles.y, 0.0f);
            if (camera)
            {
                cameraRotation.y = -camera.transform.eulerAngles.x;
                camera.fieldOfView = cameraFieldOfView;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 1000.0f;
            }

            playerGroundedMovement = gameObject.GetOrAddComponent<PlayerGroundedMovement>();
            playerGroundedMovement.PlayerHeight = playerHeight;
            
            playerNoClip = gameObject.GetOrAddComponent<PlayerNoClip>();
        }

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        #endregion

        #region Loop

        private void Update()
        {
            if (flyAction.action.WasPerformedThisFrame())
            {
                playerGroundedMovement.enabled = !playerGroundedMovement.enabled;
                playerNoClip.enabled = !playerGroundedMovement.enabled;
            }
            
            PassInputs();
        }

        private void LateUpdate()
        {
            UpdateCamera();
        }

        private void FixedUpdate()
        {
            MoveCamera();
        }

        #endregion

        #region Managment

        private void PassInputs()
        {
            playerGroundedMovement.MoveInput = moveInput.action?.ReadValue<Vector2>() ?? Vector2.zero;
            playerGroundedMovement.JumpInput = jumpInput && jumpInput.Switch();
            if (jumpInput && jumpInput.action.WasPerformedThisFrame()) playerGroundedMovement.JumpTrigger = true;

            var moveXZ = moveInput.action?.ReadValue<Vector2>() ?? Vector2.zero;
            var moveY = jumpInput.action.ReadValue<float>() - crouchAction.action.ReadValue<float>();
            playerNoClip.MoveInput = new Vector3(moveXZ.x, moveY, moveXZ.y);
        }

        #endregion

        #region Camera

        public void UpdateCamera()
        {
            var mouse = Mouse.current;
            var delta = mouse?.delta.ReadValue() * mouseSensitivity ?? Vector2.zero;
            delta += lookAction.action?.ReadValue<Vector2>() * controllerSensitivity * 480.0f * Time.deltaTime ?? Vector2.zero;

            cameraRotation += delta;
            cameraRotation.x %= 360.0f;
            cameraRotation.y = Mathf.Clamp(cameraRotation.y, -90.0f, 90.0f);

            transform.rotation = Quaternion.Euler(0.0f, cameraRotation.x, 0.0f);
            MoveCamera();
        }

        public void MoveCamera()
        {
            camera.transform.position = transform.position + Vector3.up * (playerHeight + cameraOffset);
            camera.transform.rotation = Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0.0f);
        }

        #endregion
    }
}