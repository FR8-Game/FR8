using FR8.Level;
using FR8.Player.Submodules;
using UnityEditor;
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
        [SerializeField] private float maxGroundedSpeed = 4.0f;

        [SerializeField] private float accelerationTime = 0.12f;
        [SerializeField] private float sprintSpeedScalar = 2.0f;

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

        [Space]
        [SerializeField] private float ladderClimbSpeed = 5.0f;
        [SerializeField] private float ladderRungDistance = 0.4f;
        [SerializeField] private float ladderClimbSpring = 300.0f;
        [SerializeField] private float ladderClimbDamper = 15.0f;
        [SerializeField] private float ladderJumpForce = 5.0f;

        [Header("Camera")]
        [SerializeField] private Vector3 cameraOffset = new(0.0f, 1.6f, 0.0f);

        [SerializeField] private PlayerGroundedCamera cameraController;

        private bool jumpTrigger;
        private float lastJumpTime;

        private Rigidbody lastGroundObject;
        private Vector3 lastGroundVelocity;
        private ParticleSystem pee;

        private float targetLadderPosition;

        private Pose cameraPose;

        public Transform CameraTarget { get; private set; }
        public bool IsOnGround { get; private set; }
        public RaycastHit GroundHit { get; private set; }
        public Vector3 Velocity => IsOnGround && GroundHit.rigidbody ? Rigidbody.velocity - GroundHit.rigidbody.GetPointVelocity(Rigidbody.position) : Rigidbody.velocity;
        public Vector3 Gravity => new Vector3(0.0f, -9.81f, 0.0f) * (Velocity.y > 0.0f && Controller.Jump ? upGravityScale : downGravityScale);
        public Ladder Ladder { get; private set; }
        public Ray LookingRay => new Ray(CameraTarget.position, CameraTarget.forward);

        public Vector3 MoveDirection
        {
            get
            {
                var input = Controller.Move;
                return transform.TransformDirection(input.x, 0.0f, input.z);
            }
        }

        public float MoveSpeed
        {
            get
            {
                var velocity = Velocity;
                return Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
            }
        }

        public float MaxSpeed => maxGroundedSpeed;

        #region Initalization

        protected override void Awake()
        {
            CameraTarget = transform.Find("Camera Target");
            CameraTarget.transform.localPosition = cameraOffset;
            CameraTarget.transform.localRotation = Quaternion.identity;

            base.Awake();
            cameraController.Initialize(() => Controller, CameraTarget);

            pee = transform.Find("Pee").GetComponent<ParticleSystem>();
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

            if (Controller.Pee && !pee.isPlaying)
            {
                pee.Play();
            }
            if (!Controller.Pee && pee.isPlaying)
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

            cameraController.FixedUpdate();

            if (LookForLadder()) return;
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

        private bool LookForLadder()
        {
            if (!Ladder) return false;

            var dot = -Vector3.Dot(Rigidbody.transform.forward, Ladder.transform.forward);
            var direction = Mathf.Round(Controller.Move.z) * Mathf.Sign(dot);
            var delta = direction * ladderClimbSpeed * Time.deltaTime;
            
            var overTop = targetLadderPosition + delta > Ladder.Height;
            var overBottom = targetLadderPosition + delta < 0.0f;
            var dismount = overTop || overBottom;
            if (jumpTrigger)
            {
                dismount = true;
                Rigidbody.AddForce((Ladder.transform.forward + Vector3.up) * ladderJumpForce, ForceMode.VelocityChange);
            }

            if (dismount)
            {
                if (overTop) Rigidbody.AddForce(Vector3.up * ladderJumpForce, ForceMode.VelocityChange);
                
                Ladder = null;
                return false;
            }
            
            jumpTrigger = false;

            targetLadderPosition += delta;
            targetLadderPosition = Mathf.Clamp(targetLadderPosition, 0.0f, Ladder.Height);
            var targetPosition = Ladder.ToWorldPos(Mathf.Round(targetLadderPosition / ladderRungDistance) * ladderRungDistance);

            var force = (targetPosition - Rigidbody.position) * ladderClimbSpring + (Ladder.Velocity - Rigidbody.velocity) * ladderClimbDamper;
            Rigidbody.AddForce(force);

            return true;
        }

        private void CheckForGround()
        {
            var distance = 1.0f - radius;
            IsOnGround = false;

            var ray = new Ray(transform.position + Vector3.up, Vector3.down);

            var res = Physics.SphereCastAll(ray, radius * 0.25f, distance, ~0, QueryTriggerInteraction.Ignore);
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

            ApplyGroundSpring(contraction);
        }

        private void ApplyGroundSpring(float contraction)
        {
            if (Time.time - lastJumpTime < 0.08f) return;

            var spring = contraction * groundSpring - Velocity.y * groundDamping;
            var force = Vector3.up * spring;
            Rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Move()
        {
            var moveSpeed = maxGroundedSpeed;
            if (Controller.Sprint) moveSpeed *= sprintSpeedScalar;

            var target = MoveDirection * moveSpeed;

            var difference = target - Velocity;
            difference.y = 0.0f;

            var acceleration = 1.0f / accelerationTime;
            if (!IsOnGround) acceleration *= 1.0f - airMovePenalty;
            if (Controller.Sprint) acceleration *= sprintSpeedScalar;

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
            lastJumpTime = Time.time;
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
            }

            lastGroundObject = groundObject;
        }

        #endregion

        public void SetLadder(Ladder ladder)
        {
            Ladder = ladder;
            targetLadderPosition = ladder.FromWorldPos(Rigidbody.position);
        }
    }
}