using UnityEngine;

namespace FR8.Utility
{
    [System.Serializable]
    public sealed class DampedSpring
    {
        public float springConstant = 350.0f;
        public float dampingConstant = 25.0f;
        public bool clamped = true;
        public Vector2 range = Vector2.up;
        public float bounce = 0.4f;

        [HideInInspector]public float currentPosition;
        [HideInInspector]public float targetPosition;
        [HideInInspector]public float velocity;

        public DampedSpring Set(float initialPosition)
        {
            currentPosition = initialPosition;
            targetPosition = initialPosition;
            velocity = 0.0f;
            
            return this;
        }
        
        public DampedSpring Target(float targetPosition)
        {
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
    }
}
