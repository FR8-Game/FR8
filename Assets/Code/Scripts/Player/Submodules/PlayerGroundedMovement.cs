using FMODUnity;
using FR8.Level;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FR8.Player.Submodules
{
    [System.Serializable]
    public sealed class PlayerGroundedMovement
    {
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
        public float ladderClimbSpring = 300.0f;
        public float ladderClimbDamper = 15.0f;
        public float ladderJumpForce = 5.0f;
        
        [Header("Audio")]
        public EventReference footstepSound;
        public float footstepFrequency;
        
        private PlayerAvatar avatar;
        
        private bool jumpTrigger;
        private float lastJumpTime;
        
        private Rigidbody lastGroundObject;
        private Vector3 lastGroundVelocity;
        private bool wasOnGround;
        
        private float targetLadderPosition;

        private float footstepCounter;

        public Rigidbody Rigidbody => avatar.Rigidbody;
        public PlayerInput input => avatar.input;

        public bool IsOnGround { get; private set; }
        public RaycastHit GroundHit { get; private set; }
        public Ladder Ladder { get; private set; }
        public Vector3 Velocity => IsOnGround && GroundHit.rigidbody ? Rigidbody.velocity - GroundHit.rigidbody.GetPointVelocity(Rigidbody.position) : Rigidbody.velocity;
        public Vector3 Gravity => new Vector3(0.0f, -9.81f, 0.0f) * (Velocity.y > 0.0f && input.Jump ? upGravityScale : downGravityScale);
        public bool Enabled { get; set; }
        
        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;

            avatar.UpdateEvent += Update;
            avatar.FixedUpdateEvent += FixedUpdate;
        }

        private void Update()
        {
            if (input.JumpTriggered) jumpTrigger = true;
        }

        private void FixedUpdate()
        {
            if (LookForLadder()) return;
            Move();
            Jump();
            CheckForGround();
            ApplyGravity();
            MoveWithGround();
            
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

            wasOnGround = IsOnGround;
        }
        
        private bool LookForLadder()
        {
            if (!Ladder) return false;

            var dot = -Vector3.Dot(Rigidbody.transform.forward, Ladder.transform.forward);
            var direction = Mathf.Round(input.Move.z) * Mathf.Sign(dot);
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
            var transform = avatar.transform;
            var distance = 1.0f - avatar.Radius;
            IsOnGround = false;

            var ray = new Ray(transform.position + Vector3.up, Vector3.down);

            var res = Physics.SphereCastAll(ray, avatar.Radius * 0.25f, distance, ~0, QueryTriggerInteraction.Ignore);
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
            if (input.Sprint) moveSpeed *= sprintSpeedScalar;

            var target = avatar.MoveDirection * moveSpeed;

            var difference = target - Velocity;
            difference.y = 0.0f;

            var acceleration = 1.0f / accelerationTime;
            if (!IsOnGround) acceleration *= 1.0f - airMovePenalty;
            if (input.Sprint) acceleration *= sprintSpeedScalar;

            var force = Vector3.ClampMagnitude(difference, moveSpeed) * acceleration;
            Rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Jump()
        {
            var jump = jumpTrigger;
            jumpTrigger = false;

            if (!IsOnGround) return;
            if (!jump) return;

            var power = Mathf.Sqrt(Mathf.Max(2.0f * 9.81f * upGravityScale * (jumpHeight - avatar.StepHeight), 0.0f)) - Velocity.y;
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
        
        public void SetLadder(Ladder ladder)
        {
            Ladder = ladder;
            targetLadderPosition = ladder.FromWorldPos(Rigidbody.position);
        }
    }
}