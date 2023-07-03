using System;
using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerGroundedMovement : MonoBehaviour
    {
        [Header("Physics")]
        [SerializeField] private float mass = 80.0f;

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

        private new Rigidbody rigidbody;
        private new CapsuleCollider collider;

        public Vector2 MoveInput { get; set; }
        public bool JumpTrigger { get; set; }
        public bool JumpInput { get; set; }
        public float PlayerHeight { get; set; }

        public bool IsOnGround { get; private set; }
        public RaycastHit GroundHit { get; private set; }

        public Vector3 LocalVelocity => IsOnGround && GroundHit.rigidbody ? rigidbody.velocity - GroundHit.rigidbody.GetPointVelocity(transform.position) : rigidbody.velocity;
        public Vector3 Gravity => Physics.gravity * (LocalVelocity.y > 0.0f && JumpInput ? upGravityScale : downGravityScale);

        #region Initalization

        private void OnEnable()
        {
            Configure();
        }

        private void OnDisable()
        {
            collider.enabled = false;
        }

        private void OnValidate()
        {
            Configure();
        }

        public void Configure()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.mass = mass;
            rigidbody.useGravity = true;
            rigidbody.detectCollisions = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var groundOffset = stepHeight;

            collider = gameObject.GetOrAddComponent<CapsuleCollider>();
            collider.enabled = true;
            collider.height = PlayerHeight - groundOffset;
            collider.radius = radius;
            collider.center = Vector3.up * (PlayerHeight + groundOffset) / 2.0f;
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
            
            var ray = new Ray(transform.position + Vector3.up, Vector3.down);
            if (!Physics.SphereCast(ray, radius, out var hit, distance)) return;

            var groundAngle = Mathf.Acos(Mathf.Min(hit.normal.y, 1.0f)) * Mathf.Rad2Deg;
            if (groundAngle > maxWalkableSlope) return;
            
            GroundHit = hit;
            IsOnGround = true;

            var contraction = 1.0f - GroundHit.distance / distance;

            var spring = contraction * groundSpring - LocalVelocity.y * groundDamping;
            var force = Vector3.up * spring;
            rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Move()
        {
            var input = Vector2.ClampMagnitude(MoveInput, 1.0f);
            var target = transform.TransformDirection(input.x, 0.0f, input.y) * moveSpeed;

            var difference = target - LocalVelocity;
            difference.y = 0.0f;

            var acceleration = 1.0f / accelerationTime;
            if (!IsOnGround) acceleration *= 1.0f - airMovePenalty;

            var force = Vector3.ClampMagnitude(difference, moveSpeed) * acceleration;
            rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Jump()
        {
            var jump = JumpTrigger;
            JumpTrigger = false;

            if (!IsOnGround) return;
            if (!jump) return;

            var power = Mathf.Sqrt(Mathf.Max(2.0f * -Physics.gravity.y * upGravityScale * jumpHeight, 0.0f)) - LocalVelocity.y;
            var force = Vector3.up * power;

            rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private void ApplyGravity()
        {
            if (!rigidbody.useGravity) return;

            rigidbody.AddForce(Gravity - Physics.gravity, ForceMode.Acceleration);
        }

        #endregion
    }
}