using UnityEngine;

namespace FR8Runtime.CodeUtility
{
    public static class Physics
    {
        public static (Vector3, Vector3) CalculateDampedSpring(SpringBody current, SpringBody target, SpringSettings settings)
        {
            var force = (target.position - current.position) * settings.spring + (target.velocity - current.velocity) * settings.damper;

            var (angle, axis) = Quaternion.ToAngleAxis(target.rotation * UnityEngine.Quaternion.Inverse(current.rotation));
            var torque = (axis * angle * Mathf.Deg2Rad) * settings.spring + (target.angularVelocity - current.angularVelocity) * settings.damper;
            torque *= settings.torqueScale;

            force -= UnityEngine.Physics.gravity;

            return (force, torque);
        }

        [System.Serializable]
        public class SpringBody
        {
            public Vector3 position;
            public UnityEngine.Quaternion rotation;
            public Vector3 velocity;
            public Vector3 angularVelocity;

            public SpringBody() : this(Vector3.zero, UnityEngine.Quaternion.identity) { }
            public SpringBody(Vector3 position, UnityEngine.Quaternion rotation) : this(position, rotation, Vector3.zero, Vector3.zero) { }
            public SpringBody(Transform transform) : this(transform.position, transform.rotation) { }
            public SpringBody(Rigidbody body) : this(body.position, body.rotation, body.velocity, body.angularVelocity) { }

            public SpringBody(Vector3 position, UnityEngine.Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
            {
                this.position = position;
                this.rotation = rotation;
                this.velocity = velocity;
                this.angularVelocity = angularVelocity;
            }
        }

        [System.Serializable]
        public class SpringSettings
        {
            public float spring, damper, torqueScale;

            public SpringSettings() { }

            public SpringSettings(float spring, float damper, float torqueScale)
            {
                this.spring = spring;
                this.damper = damper;
                this.torqueScale = torqueScale;
            }
        }
    }
}