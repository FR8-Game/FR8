using System;
using FMODUnity;
using FR8.Level;
using FR8.Player.Submodules;
using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerAvatar : MonoBehaviour
    {
        [Header("Input")]
        public PlayerInput input;
        
        [Header("Physics")]
        [SerializeField] private float mass = 80.0f;
        [SerializeField] private float playerHeight = 1.7f;
        [SerializeField] private float radius = 0.25f;
        [SerializeField] private float stepHeight = 0.5f;

        public PlayerGroundedMovement groundedMovement;
        public PlayerInteractionManager interactionManager;
        
        [Header("Camera")]
        public Vector3 cameraOffset = new(0.0f, 1.6f, 0.0f);
        public PlayerCamera cameraController;
        
        private ParticleSystem pee;
        
        private Pose cameraPose;

        public event Action EnabledEvent;
        public event Action UpdateEvent;
        public event Action FixedUpdateEvent;
        public event Action DisabledEvent;
        
        public Rigidbody Rigidbody { get; private set; }
        public Transform CameraTarget { get; private set; }
        public Ray LookingRay => new(CameraTarget.position, CameraTarget.forward);
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

        public float MoveSpeed
        {
            get
            {
                var velocity = groundedMovement.Velocity;
                return Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
            }
        }

        #region Initalization

        private void Awake()
        {
            transform.SetParent(null);
            
            input.Init();
            
            CameraTarget = transform.Find("Camera Target");
            CameraTarget.transform.localPosition = cameraOffset;
            CameraTarget.transform.localRotation = Quaternion.identity;

            cameraController.Init(this, CameraTarget);
            interactionManager.Init(this);

            groundedMovement.Init(this);
            
            pee = transform.Find("Pee").GetComponent<ParticleSystem>();
        }

        private void OnValidate()
        {
            Configure();
        }

        private void OnEnable()
        {
            Configure();
            EnabledEvent?.Invoke();
        }

        private void OnDisable()
        {
            DisabledEvent?.Invoke();
        }

        private void Configure()
        {
            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            
            Rigidbody.mass = mass;
            Rigidbody.useGravity = false;
            Rigidbody.detectCollisions = true;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            Rigidbody.interpolation = RigidbodyInterpolation.None;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var groundOffset = stepHeight;

            var collider = gameObject.GetOrAddComponent<CapsuleCollider>();
            collider.enabled = true;
            collider.height = playerHeight - groundOffset;
            collider.radius = radius;
            collider.center = Vector3.up * (playerHeight + groundOffset) / 2.0f;
            if (Application.isPlaying)
            {
                var mat = new PhysicMaterial("[PROC] Player Physics Material");
                mat.bounciness = 0.0f;
                mat.dynamicFriction = 0.0f;
                mat.staticFriction = 0.0f;

                mat.bounceCombine = PhysicMaterialCombine.Multiply;
                mat.frictionCombine = PhysicMaterialCombine.Multiply;

                if (collider.material) Destroy(collider.material);
                collider.material = mat;
            }
        }

        #endregion

        #region Loop

        private void Update()
        {
            UpdateEvent?.Invoke();

            if (input.Pee && !pee.isEmitting)
            {
                pee.Play();
            }

            if (!input.Pee && pee.isEmitting)
            {
                pee.Stop();
            }

            pee.transform.localRotation = Quaternion.Euler(-cameraController.Yaw * 0.5f, 0.0f, 0.0f);
        }

        private void FixedUpdate()
        {
            Rigidbody.rotation = Quaternion.Euler(0.0f, CameraTarget.eulerAngles.y, 0.0f);
            CameraTarget.localRotation = Quaternion.identity;
            CameraTarget.localPosition = cameraOffset;

            FixedUpdateEvent?.Invoke();
        }

        #endregion
    }
}