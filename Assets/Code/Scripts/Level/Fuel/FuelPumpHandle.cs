using FR8.Pickups;
using FR8.Player.Submodules;
using FR8.Sockets;
using UnityEngine;

namespace FR8.Level.Fuel
{
    [SelectionBase, DisallowMultipleComponent]
    public class FuelPumpHandle : PickupObject, ISocketable
    {
        public static readonly Utility.Physics.SpringSettings SpringSettings = new()
        {
            spring = 300.0f,
            damper = 18.0f,
            torqueScale = 1.0f,
        };

        private Vector3 velocity, angularVelocity;

        private SocketManager currentBinding;
        public string SocketType => "FuelPump";

        public bool CanBind() => !currentBinding && !Held;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!currentBinding) return;

            var current = new Utility.Physics.SpringBody(transform.position, transform.rotation, velocity, angularVelocity);
            var target = new Utility.Physics.SpringBody(currentBinding.transform.position, currentBinding.transform.rotation);

            var (force, torque) = Utility.Physics.CalculateDampedSpring(current, target, SpringSettings);

            transform.position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;

            transform.rotation = Quaternion.Euler(angularVelocity * Time.deltaTime) * transform.rotation;
            angularVelocity += torque * Time.deltaTime;
        }

        public ISocketable Bind(SocketManager manager)
        {
            if (!CanBind()) return null;

            LockRigidbody(true);

            currentBinding = manager;
            return this;
        }

        public ISocketable Unbind()
        {
            LockRigidbody(false);

            currentBinding = null;
            return null;
        }

        public override PickupObject Pickup(PlayerInteractionManager target)
        {
            if (currentBinding) currentBinding.Unbind();
            return base.Pickup(target);
        }

        public void LockRigidbody(bool state)
        {
            Rigidbody.isKinematic = state;

            if (state)
            {
                velocity = Rigidbody.velocity;
                angularVelocity = Rigidbody.angularVelocity;
            }
            else
            {
                Rigidbody.velocity = velocity;
                Rigidbody.angularVelocity = angularVelocity;
            }
        }
    }
}