using System;
using FR8Runtime.Player.Submodules;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerAvatar : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float mass = 80.0f;

        [SerializeField] private float playerHeight = 1.7f;
        [SerializeField] private float radius = 0.25f;
        [SerializeField] private float stepHeight = 0.5f;

        public PlayerInput input;
        public PlayerGroundedMovement groundedMovement;
        public PlayerInteractionManager interactionManager;
        public PlayerCamera cameraController;
        public PlayerUrination urination;

        public event Action EnabledEvent;
        public event Action UpdateEvent;
        public event Action FixedUpdateEvent;
        public event Action DisabledEvent;

        public Rigidbody Rigidbody { get; private set; }

        public float Radius => radius;
        public float StepHeight => stepHeight;

        public Vector3 MoveDirection
        {
            get
            {
                var input = this.input.Move;
                return transform.TransformDirection(input.x, 0.0f, input.z);
            }
        }

        private void Awake()
        {
            transform.SetParent(null);
            InitSubmodules();
        }

        private void InitSubmodules()
        {
            input.Init();
            cameraController.Init(this);
            interactionManager.Init(this);
            groundedMovement.Init(this);
            urination.Init(this);
        }

        private void OnEnable()
        {
            ConfigureAll();
            EnabledEvent?.Invoke();
        }

        private void OnDisable()
        {
            DisabledEvent?.Invoke();
        }

        private void ConfigureAll()
        {
            ConfigureRigidbody();

            ConfigureCollider();
        }

        private void ConfigureCollider()
        {
            var groundOffset = stepHeight;

            var collider = gameObject.GetOrAddComponent<CapsuleCollider>();
            collider.enabled = true;
            collider.height = playerHeight - groundOffset;
            collider.radius = radius;
            collider.center = Vector3.up * (playerHeight + groundOffset) / 2.0f;


            if (collider.material) Destroy(collider.material);
            collider.material = CreatePlayerPhysicsMaterial();
        }

        private static PhysicMaterial CreatePlayerPhysicsMaterial()
        {
            var mat = new PhysicMaterial("[PROC] Player Physics Material");
            mat.hideFlags = HideFlags.HideAndDontSave;
            mat.bounciness = 0.0f;
            mat.dynamicFriction = 0.0f;
            mat.staticFriction = 0.0f;

            mat.bounceCombine = PhysicMaterialCombine.Multiply;
            mat.frictionCombine = PhysicMaterialCombine.Multiply;
            return mat;
        }

        private void ConfigureRigidbody()
        {
            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();

            Rigidbody.mass = mass;
            Rigidbody.useGravity = false;
            Rigidbody.detectCollisions = true;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            Rigidbody.interpolation = RigidbodyInterpolation.None;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
        }

        private void FixedUpdate()
        {
            SyncWithCameraTarget();

            FixedUpdateEvent?.Invoke();
        }

        private void SyncWithCameraTarget()
        {
            Rigidbody.rotation = Quaternion.Euler(0.0f, cameraController.CameraTarget.eulerAngles.y, 0.0f);
            cameraController.CameraTarget.localRotation = Quaternion.identity;
            cameraController.CameraTarget.localPosition = cameraController.cameraOffset;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;

            CodeUtility.GizmoExtras.DrawCapsule(Vector3.up * playerHeight / 2.0f, Quaternion.identity, playerHeight, radius);
        }
    }
}