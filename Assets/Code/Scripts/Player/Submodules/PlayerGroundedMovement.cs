using System;
using FMODUnity;
using FR8Runtime.CodeUtility;
using FR8Runtime.Level;
using UnityEngine;

namespace FR8Runtime.Player.Submodules
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerAvatar))]
    public sealed class PlayerGroundedMovement : MonoBehaviour
    {
        [Header("Collision")]
        [SerializeField] private float mass = 80.0f;

        [SerializeField] private float collisionHeight = 1.7f;
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private float stepHeight = 0.5f;
        [SerializeField] private float crouchHeight = 0.6f;
        [SerializeField] public Vector3 cameraOffset = new(0.0f, 1.6f, 0.0f);

        [Header("Movement")]
        public float maxGroundedSpeed = 4.0f;

        public float accelerationTime = 0.12f;
        public float sprintSpeedScalar = 2.0f;

        [Range(0.0f, 1.0f)]
        public float airMovePenalty = 0.8f;

        public float jumpHeight = 2.5f;

        [Range(90.0f, 0.0f)]
        public float maxWalkableSlope = 40.0f;

        [Space]
        public float groundSpring = 500.0f;

        public float groundDamping = 25.0f;

        [Space]
        public float downGravityScale = 3.0f;

        public float upGravityScale = 2.0f;

        [Space]
        public float ladderClimbSpeed = 5.0f;

        public float ladderRungDistance = 0.4f;
        public float ladderClimbSpring = 12000.0f;
        public float ladderClimbDamper = 3000.0f;
        public float ladderJumpForce = 5.0f;

        [Header("Audio")]
        public EventReference footstepSound;

        public float footstepFrequency;

        private PlayerAvatar avatar;
        private new CapsuleCollider collider;

        private bool jumpTrigger;
        private float lastJumpTime;

        private float crouchPercentRaw;

        private Rigidbody lastGroundObject;
        private Vector3 lastGroundVelocity;
        private bool wasOnGround;

        private float targetLadderPosition;
        private float footstepCounter;

        public PlayerInput Input => avatar.input;

        private const float GroundCheckRayLength = 1.0f;
        private float GroundCheckRadius => collisionRadius * 0.25f;

        public float GroundCheckHeightOffset
        {
            get
            {
                var maxDistance = GroundCheckRayLength - GroundCheckRadius;

                var downForce = -Physics.gravity.y * downGravityScale;
                var compression = downForce / groundSpring;
                var distance = maxDistance - (1.0f - compression) * maxDistance;
                return distance;
            }
        }

        public float Mass => mass;
        public float CollisionRadius => collisionRadius;
        public float CollisionHeight => collisionHeight;
        public float StepHeight => stepHeight;

        public bool IsOnGround { get; private set; }
        public float CrouchPercent => CurvesUtility.SmootherStep(crouchPercentRaw);
        public RaycastHit GroundHit { get; private set; }
        public Ladder Ladder { get; private set; }
        public Vector3 Velocity => IsOnGround && GroundHit.rigidbody ? avatar.Body.velocity - GroundHit.rigidbody.GetPointVelocity(avatar.Body.position) : avatar.Body.velocity;
        public Vector3 Gravity => new Vector3(0.0f, -9.81f, 0.0f) * (Velocity.y > 0.0f && Input.Jump ? upGravityScale : downGravityScale);
        public bool Enabled { get; set; }

        public float MoveSpeed
        {
            get
            {
                var velocity = Velocity;
                return Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
            }
        }

        private void OnEnable()
        {
            avatar = GetComponent<PlayerAvatar>();

            ConfigureRigidbody();
            ConfigureCollider();

            avatar.getCenter = () => collider.transform.TransformPoint(collider.center);
            avatar.MountChangedEvent += OnMountChanged;

            var camera = Camera.main;
            transform.position = camera.transform.position - cameraOffset;
            transform.rotation = Quaternion.Euler(0.0f, camera.transform.eulerAngles.y, 0.0f);
        }


        private void OnDisable()
        {
            avatar.Body.isKinematic = true;
            avatar.MountChangedEvent -= OnMountChanged;
            Destroy(collider);
        }

        private void OnMountChanged(PlayerMount newMount)
        {
            if (!newMount && avatar.CurrentMount)
            {
                avatar.Body.position = avatar.CurrentMount.DismountPosition;
            }
        }

        private void ConfigureRigidbody()
        {
            avatar.Body.mass = mass;
            avatar.Body.isKinematic = false;
            avatar.Body.useGravity = false;
            avatar.Body.detectCollisions = true;
            avatar.Body.constraints = RigidbodyConstraints.FreezeRotation;
            avatar.Body.interpolation = RigidbodyInterpolation.None;
            avatar.Body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            avatar.Body.velocity = Vector3.zero;
            avatar.Body.angularVelocity = Vector3.zero;
        }

        public void ConfigureCollider()
        {
            var gameObject = avatar.gameObject;
            var groundOffset = stepHeight;

            var height = Mathf.Lerp(collisionHeight, crouchHeight, CrouchPercent);

            if (!collider)
            {
                collider = gameObject.AddComponent<CapsuleCollider>();
                if (collider.material) Destroy(collider.material);
                collider.material = CreatePlayerPhysicsMaterial();
            }

            collider.enabled = true;
            collider.height = height - groundOffset;
            collider.height = height - groundOffset;
            collider.radius = collisionRadius;
            collider.center = Vector3.up * (height + groundOffset) / 2.0f;
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

        private void Update()
        {
            ConstrainTransform();
            SetJumpTrigger();
        }

        private void SetJumpTrigger()
        {
            if (Input.JumpTriggered) jumpTrigger = true;
        }

        private void FixedUpdate()
        {
            if (avatar.CurrentMount)
            {
                avatar.Body.position = avatar.CurrentMount.Position;
                avatar.Body.rotation = avatar.CurrentMount.Rotation;
                avatar.Body.velocity = avatar.CurrentMount.Velocity;
                avatar.Body.detectCollisions = false;

                if (avatar.input.Crouch)
                {
                    avatar.SetMount(null);
                }

                return;
            }

            avatar.Body.detectCollisions = true;

            if (LookForLadder()) return;

            Crouch();
            Move();
            Jump();
            CheckForGround();
            ApplyGravity();
            MoveWithGround();

            //PlayFootstepAudio();

            UpdateFlags();
        }

        private void ConstrainTransform()
        {
            if (avatar.CurrentMount)
            {
                avatar.Head.rotation = avatar.CurrentMount.ConstrainRotation(avatar.Head.rotation);
                return;
            }

            var rotation = avatar.Head.rotation;
            transform.rotation = Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
            avatar.Head.rotation = rotation;
        }

        private void Crouch()
        {
            ConfigureCollider();

            var isCrouching = Input.Crouch;

            crouchPercentRaw += ((isCrouching ? 1.0f : 0.0f) - crouchPercentRaw) * 6.0f * Time.deltaTime;

            var offset = Vector3.down * (collisionHeight - crouchHeight);
            avatar.Head.localPosition = cameraOffset + offset * CrouchPercent;
        }

        private void PlayFootstepAudio()
        {
            if (IsOnGround)
            {
                if (!wasOnGround)
                {
                    footstepSound.OneShot(avatar.gameObject);
                }
                else
                {
                    var v = Velocity;
                    var s = Mathf.Sqrt(v.x * v.x + v.z * v.z) / maxGroundedSpeed;
                    footstepCounter += s * footstepFrequency * Time.deltaTime;
                    if (footstepCounter > 1.0f)
                    {
                        footstepCounter -= 1.0f;
                        footstepSound.OneShot(avatar.gameObject);
                    }
                }
            }
        }

        private void UpdateFlags()
        {
            wasOnGround = IsOnGround;
        }

        private bool LookForLadder()
        {
            if (!Ladder) return false;

            var delta = GetLadderClimbDelta();

            if (TryDismountLadder(delta)) return false;

            jumpTrigger = false;

            IncrementLadderPosition(delta);
            ApplyLadderCorrectiveForce();
            return true;
        }

        private float GetLadderClimbDelta()
        {
            var dot = -Vector3.Dot(avatar.Body.transform.forward, Ladder.transform.forward);
            var direction = Mathf.Round(Input.Move.z) * Mathf.Sign(dot);
            var delta = direction * ladderClimbSpeed * Time.deltaTime;
            return delta;
        }

        private bool WantsToDismount(float delta, out bool overTop)
        {
            overTop = targetLadderPosition + delta > Ladder.Height;
            var overBottom = targetLadderPosition + delta < 0.0f;
            var dismount = overTop || overBottom;
            if (jumpTrigger)
            {
                dismount = true;
                avatar.Body.AddForce((Ladder.transform.forward + Vector3.up) * ladderJumpForce, ForceMode.VelocityChange);
            }

            return dismount;
        }

        private bool TryDismountLadder(float delta)
        {
            if (!WantsToDismount(delta, out var overTop)) return false;

            if (overTop) avatar.Body.AddForce(Vector3.up * ladderJumpForce, ForceMode.VelocityChange);

            Ladder = null;
            return true;
        }

        private void IncrementLadderPosition(float delta)
        {
            targetLadderPosition += delta;
            targetLadderPosition = Mathf.Clamp(targetLadderPosition, 0.0f, Ladder.Height);
        }

        private void ApplyLadderCorrectiveForce()
        {
            var targetPosition = Ladder.ToWorldPos(Mathf.Round(targetLadderPosition / ladderRungDistance) * ladderRungDistance);

            var force = (targetPosition - avatar.Body.position) * ladderClimbSpring + (Ladder.Velocity - avatar.Body.velocity) * ladderClimbDamper;
            avatar.Body.AddForce(force);
        }

        private void CheckForGround()
        {
            var distance = GroundCheckRayLength - GroundCheckRadius;
            IsOnGround = false;

            var ray = new Ray(transform.position + Vector3.up * (1.0f - GroundCheckHeightOffset), Vector3.down);

            var res = Physics.SphereCastAll(ray, GroundCheckRadius, distance, ~0, QueryTriggerInteraction.Ignore);
            if (res.Length == 0) return;

            if (!GetValidGroundHit(res, out var bestHit)) return;

            GroundHit = bestHit;
            IsOnGround = true;

            ApplyGroundSpring(distance);
        }

        private bool GetValidGroundHit(RaycastHit[] hits, out RaycastHit bestHit)
        {
            var res = false;
            bestHit = default;

            foreach (var hit in hits)
            {
                // --- Validation Checks ---
                // Check that the hit object is not ourselves
                if (hit.transform.IsChildOf(transform)) continue;

                // Check the angle of the ground is not too steep to stand on
                var groundAngle = Mathf.Acos(Mathf.Min(transform.InverseTransformDirection(hit.normal).y, 1.0f)) * Mathf.Rad2Deg;
                if (groundAngle > maxWalkableSlope) continue;

                // Discard result if bestHit is closer.
                if (res && bestHit.distance < hit.distance) continue;

                res = true;
                bestHit = hit;
            }

            return res;
        }

        private void ApplyGroundSpring(float distance)
        {
            var contraction = GroundCheckRayLength - GroundHit.distance / distance;

            if (Time.time - lastJumpTime < 0.08f) return;

            var spring = contraction * groundSpring - Velocity.y * groundDamping;
            var force = Vector3.up * spring;
            avatar.Body.AddForce(force, ForceMode.Acceleration);
        }

        private void Move()
        {
            var (target, moveSpeed) = GetTargetMoveVelocity();

            var difference = target - Velocity;
            difference.y = 0.0f;

            var acceleration = GetMoveAcceleration();

            var force = Vector3.ClampMagnitude(difference, moveSpeed) * acceleration;
            avatar.Body.AddForce(force, ForceMode.Acceleration);
        }

        private void Fly()
        {
            var moveSpeed = maxGroundedSpeed * sprintSpeedScalar;
            var target = avatar.MoveDirection * moveSpeed;

            if (avatar.input.Jump) target.y += moveSpeed;
            if (avatar.input.Crouch) target.y -= moveSpeed;

            var difference = target - Velocity;
            var acceleration = sprintSpeedScalar / accelerationTime;

            var force = Vector3.ClampMagnitude(difference, moveSpeed) * acceleration;
            avatar.Body.AddForce(force, ForceMode.Acceleration);
        }

        private float GetMoveAcceleration()
        {
            var acceleration = 1.0f / accelerationTime;
            if (!IsOnGround) acceleration *= 1.0f - airMovePenalty;
            if (Input.Sprint) acceleration *= sprintSpeedScalar;
            return acceleration;
        }

        private (Vector3, float) GetTargetMoveVelocity()
        {
            var moveSpeed = maxGroundedSpeed;
            if (Input.Sprint) moveSpeed *= sprintSpeedScalar;

            var target = avatar.MoveDirection * moveSpeed;
            return (target, moveSpeed);
        }

        private void Jump()
        {
            var jump = jumpTrigger;
            jumpTrigger = false;

            if (!IsOnGround) return;
            if (!jump) return;

            avatar.Body.AddForce(CalculateJumpForce(), ForceMode.VelocityChange);
            lastJumpTime = Time.time;
        }

        private Vector3 CalculateJumpForce()
        {
            var power = Mathf.Sqrt(Mathf.Max(2.0f * 9.81f * upGravityScale * (jumpHeight - stepHeight), 0.0f)) - Velocity.y;
            var force = Vector3.up * power;
            return force;
        }

        private void ApplyGravity()
        {
            avatar.Body.AddForce(Gravity, ForceMode.Acceleration);
        }

        private void MoveWithGround()
        {
            var groundObject = GroundHit.rigidbody;

            if (groundObject)
            {
                var velocity = groundObject.GetPointVelocity(avatar.Body.position);
                if (groundObject == lastGroundObject)
                {
                    var deltaVelocity = velocity - lastGroundVelocity;
                    var deltaRotation = groundObject.angularVelocity * Mathf.Rad2Deg * Time.deltaTime;

                    var force = deltaVelocity / Time.deltaTime;

                    avatar.Body.AddForce(force, ForceMode.Acceleration);
                    avatar.Body.MoveRotation(avatar.Body.rotation * Quaternion.Euler(deltaRotation));
                }

                lastGroundVelocity = velocity;
            }

            lastGroundObject = groundObject;
        }

        public void SetLadder(Ladder ladder)
        {
            Ladder = ladder;
            targetLadderPosition = ladder.FromWorldPos(avatar.Body.position);
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;

            GizmoUtility.DrawCapsule(Vector3.up * collisionHeight / 2.0f, Quaternion.identity, collisionHeight, collisionRadius);
        }
    }
}