using FR8.Player.Submodules;
using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerGroundedAvatar : PlayerAvatar
    {
        [Header("Physics")]
        [SerializeField] private float mass = 80.0f;

        [SerializeField] private float playerHeight = 1.7f;
        [SerializeField] private float radius = 0.25f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8.0f;

        [SerializeField] private float accelerationTime = 0.12f;

        [Range(0.0f, 1.0f)]
        [SerializeField] private float airMovePenalty = 0.8f;

        [SerializeField] private float jumpHeight = 2.5f;
        [SerializeField] private float stepHeight = 0.5f;

        [Range(90.0f, 0.0f)]
        [SerializeField] private float maxWalkableSlope = 40.0f;

        [Space]
        [SerializeField] private float groundSpring = 500.0f;

        [SerializeField] private float groundDamping = 25.0f;

        [Space]
        [SerializeField] private float downGravityScale = 3.0f;

        [SerializeField] private float upGravityScale = 2.0f;

        [Header("Camera")]
        [SerializeField] private Vector3 cameraOffset = new(0.0f, 1.6f, 0.0f);

        [SerializeField] private PlayerGroundedCamera cameraController;

        private bool jumpTrigger;

        private Rigidbody lastGroundObject;
        private Vector3 lastGroundVelocity;
        private Quaternion lastGroundRotation = Quaternion.identity;

        private Pose cameraPose;

        public Transform CameraTarget { get; private set; }
        public bool IsOnGround { get; private set; }
        public RaycastHit GroundHit { get; private set; }
        public Vector3 Velocity => IsOnGround && GroundHit.rigidbody ? Rigidbody.velocity - GroundHit.rigidbody.GetPointVelocity(Rigidbody.position) : Rigidbody.velocity;
        public Vector3 Gravity => new Vector3(0.0f, -9.81f, 0.0f) * (Velocity.y > 0.0f && Controller.Jump ? upGravityScale : downGravityScale);

        #region Initalization

        protected override void Awake()
        {
            CameraTarget = transform.Find("Camera Target");
            CameraTarget.transform.localPosition = cameraOffset;
            CameraTarget.transform.localRotation = Quaternion.identity;

            base.Awake();
            cameraController.Initialize(() => Controller, CameraTarget);
        }

        private void OnValidate()
        {
            Configure();
        }

        private void OnEnable()
        {
            Configure();
            cameraController.OnEnable();
        }

        private void OnDisable()
        {
            cameraController.OnDisable();
        }

        protected override void Configure()
        {
            base.Configure();

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
            if (Controller.JumpTriggered) jumpTrigger = true;

            cameraController.Update();
        }

        private void FixedUpdate()
        {
            Rigidbody.rotation = Quaternion.Euler(0.0f, CameraTarget.eulerAngles.y, 0.0f);
            CameraTarget.localRotation = Quaternion.identity;
            CameraTarget.localPosition = cameraOffset;
            
            CheckForGround();
            Move();
            Jump();
            ApplyGravity();
            MoveWithGround();
        }

        private void GetCameraPose()
        {
            if (!IsOnGround) return;
        }

        #endregion

        #region Physics

        private void CheckForGround()
        {
            var distance = 1.0f - radius;
            IsOnGround = false;

            var ray = new Ray(transform.position + Vector3.up, Vector3.down);

            var res = Physics.SphereCastAll(ray, radius * 0.25f, distance);
            if (res.Length == 0) return;

            RaycastHit? bestHit = null;
            foreach (var hit in res)
            {
                // --- Validation Checks ---
                // Check that the hit object is not ourselves
                if (hit.transform.IsChildOf(transform)) continue;

                // Check the angle of the ground is not too steep to stand on
                var groundAngle = Mathf.Acos(Mathf.Min(transform.InverseTransformDirection(hit.normal).y, 1.0f)) * Mathf.Rad2Deg;
                if (groundAngle > maxWalkableSlope) continue;

                // Discard result if bestHit is closer.
                if (bestHit.HasValue && bestHit.Value.distance < hit.distance) continue;

                bestHit = hit;
            }

            if (!bestHit.HasValue) return;

            GroundHit = bestHit.Value;
            IsOnGround = true;

            var contraction = 1.0f - GroundHit.distance / distance;

            var spring = contraction * groundSpring - Velocity.y * groundDamping;
            var force = Vector3.up * spring;
            Rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Move()
        {
            var input = Controller.Move;
            var target = transform.TransformDirection(input.x, 0.0f, input.z) * moveSpeed;

            var difference = target - Velocity;
            difference.y = 0.0f;

            var acceleration = 1.0f / accelerationTime;
            if (!IsOnGround) acceleration *= 1.0f - airMovePenalty;

            var force = Vector3.ClampMagnitude(difference, moveSpeed) * acceleration;
            Rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Jump()
        {
            var jump = jumpTrigger;
            jumpTrigger = false;

            if (!IsOnGround) return;
            if (!jump) return;

            var power = Mathf.Sqrt(Mathf.Max(2.0f * 9.81f * upGravityScale * (jumpHeight - stepHeight), 0.0f)) - Velocity.y;
            var force = Vector3.up * power;

            Rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private void ApplyGravity()
        {
            Rigidbody.AddForce(Gravity, ForceMode.Acceleration);
        }

        private void MoveWithGround()
        {
            var groundObject = GroundHit.rigidbody;

            if (groundObject)
            {
                var velocity = groundObject.GetPointVelocity(Rigidbody.position);
                if (groundObject == lastGroundObject)
                {
                    var deltaVelocity = velocity - lastGroundVelocity;
                    var deltaRotation = groundObject.angularVelocity * Mathf.Rad2Deg * Time.deltaTime;

                    var force = deltaVelocity / Time.deltaTime;
                    
                    Rigidbody.AddForce(force, ForceMode.Acceleration);
                    Rigidbody.MoveRotation(Rigidbody.rotation * Quaternion.Euler(deltaRotation));
                }

                lastGroundVelocity = velocity;
                lastGroundRotation = groundObject.rotation;
            }

            lastGroundObject = groundObject;
        }

        #endregion
    }
}