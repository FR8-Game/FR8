using UnityEngine;

namespace FR8.Runtime.CodeUtility
{
    [System.Serializable]
    public sealed class DampedSpring
    {
        public float springConstant = 350.0f;
        public float dampingConstant = 25.0f;
        public bool clamped = true;
        public Vector2 range = Vector2.up;
        public float bounce = 0.4f;

        [HideInInspector] public float currentPosition;
        [HideInInspector] public float targetPosition;
        [HideInInspector] public float velocity;

        public DampedSpring Set(float initialPosition)
        {
            currentPosition = initialPosition;
            targetPosition = initialPosition;
            velocity = 0.0f;

            return this;
        }

        public DampedSpring Target(float targetPosition)
        {
            if (clamped) targetPosition = Mathf.Clamp(targetPosition, range.x, range.y);
            this.targetPosition = targetPosition;
            return this;
        }

        public DampedSpring Iterate(float deltaTime)
        {
            var force = 0.0f;

            force += (targetPosition - currentPosition) * springConstant;
            force -= velocity * dampingConstant;

            currentPosition += velocity * deltaTime;
            velocity += force * deltaTime;

            if (clamped)
            {
                Bounce();
            }

            return this;
        }

        private void Bounce()
        {
            if (currentPosition > range.y)
            {
                currentPosition = range.y;
                if (velocity > 0.0f)
                {
                    velocity = -velocity * bounce;
                }
            }

            if (currentPosition < range.x)
            {
                currentPosition = range.x;
                if (velocity < 0.0f)
                {
                    velocity = -velocity * bounce;
                }
            }
        }

        public static void ApplyForce(Rigidbody body, Vector3 position, UnityEngine.Quaternion rotation, float spring, float damper, float torqueScale, bool ignoreMass)
        {
            ApplyForce(body, position, rotation, Vector3.zero, Vector3.zero, spring, damper, torqueScale, ignoreMass);
        }

        public static void ApplyForce(Rigidbody body, Vector3 position, UnityEngine.Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float spring, float damper, float torqueScale, bool ignoreMass)
        {
            var current = new PhysicsUtility.SpringBody(body);
            var target = new PhysicsUtility.SpringBody(position, rotation, velocity, angularVelocity);
            var settings = new PhysicsUtility.SpringSettings(spring, damper, torqueScale);

            var forceMode = ignoreMass ? ForceMode.Acceleration : ForceMode.Force;

            var (force, torque) = PhysicsUtility.CalculateDampedSpring(current, target, settings);
            body.AddForce(force, forceMode);
            body.AddTorque(torque, forceMode);
        }
    }
}