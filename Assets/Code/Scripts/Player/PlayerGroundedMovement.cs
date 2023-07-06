using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerGroundedMovement : PlayerMovementModule
    {
        [Header("Physics")]
        [SerializeField] private float mass = 80.0f;
        [SerializeField] private float playerHeight = 1.7f;
        [SerializeField] private float radius = 0.25f;
        [SerializeField] private float cameraOffset = -0.1f;

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

        private new CapsuleCollider collider;
        private bool jumpTrigger;
        private float cameraYaw;

        public Vector3 MoveInput => Controller.Move;
        public bool JumpInput => Controller.Jump;
        
        public Vector3 Up => -Gravity.normalized;
        public bool IsOnGround { get; private set; }
        public RaycastHit GroundHit { get; private set; }

        public Vector3 LocalVelocity => IsOnGround && GroundHit.rigidbody ? Rigidbody.velocity - GroundHit.rigidbody.GetPointVelocity(transform.position) : Rigidbody.velocity;
        public Vector3 Gravity => GetGlobalGravity() * (LocalVelocity.y > 0.0f && JumpInput ? upGravityScale : downGravityScale);

        private Vector3 GetGlobalGravity() => Physics.gravity;

        #region Initalization

        private void OnEnable()
        {
            Configure();
            Controller.CameraOffset = Vector3.up * (playerHeight - cameraOffset);
        }

        public void Configure()
        {
            Rigidbody.mass = mass;
            Rigidbody.detectCollisions = true;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var groundOffset = stepHeight;

            collider = gameObject.GetOrAddComponent<CapsuleCollider>();
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

            var lookDelta = Controller.LookFrameDelta;
            
            var right = Vector3.Cross(transform.forward, Up).normalized;
            var forward = Vector3.Cross(Up, right).normalized;
            var baseOrientation = Quaternion.LookRotation(forward, Up);
            
            baseOrientation = baseOrientation * Quaternion.Euler(0.0f, lookDelta.x, 0.0f);
            cameraYaw += lookDelta.y;
            cameraYaw = Mathf.Clamp(cameraYaw, -90.0f, 90.0f);
            
            var cameraOrientation = baseOrientation * Quaternion.Euler(-cameraYaw, 0.0f, 0.0f);

            transform.rotation = baseOrientation;
            Controller.GlobalCameraOrientation = cameraOrientation;
        }

        private void FixedUpdate()
        {
            CheckForGround();
            Move();
            Jump();
            ApplyGravity();
        }

        #endregion

        #region Physics

        private void CheckForGround()
        {
            var distance = 1.0f - radius;
            IsOnGround = false;
            
            var ray = new Ray(transform.position + Up, -Up);
            if (!Physics.SphereCast(ray, radius, out var hit, distance)) return;

            var groundAngle = Mathf.Acos(Mathf.Min(transform.InverseTransformDirection(hit.normal).y, 1.0f)) * Mathf.Rad2Deg;
            if (groundAngle > maxWalkableSlope) return;
            
            GroundHit = hit;
            IsOnGround = true;

            var contraction = 1.0f - GroundHit.distance / distance;

            var spring = contraction * groundSpring - Vector3.Dot(Up, LocalVelocity) * groundDamping;
            var force = Up * spring;
            Rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Move()
        {
            var input = MoveInput;
            var target = transform.TransformDirection(input.x, 0.0f, input.z) * moveSpeed;

            var difference = target - LocalVelocity;
            difference -= Up * Vector3.Dot(Up, difference);

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

            var power = Mathf.Sqrt(Mathf.Max(2.0f * -Vector3.Dot(Up, Physics.gravity) * upGravityScale * jumpHeight, 0.0f)) - LocalVelocity.y;
            var force = Up * power;

            Rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private void ApplyGravity()
        {
            if (!Rigidbody.useGravity) return;

            Rigidbody.AddForce(Gravity - Physics.gravity, ForceMode.Acceleration);
        }

        #endregion
    }
}