
using System.Runtime.InteropServices;
using FR8.Pickups;
using FR8.Player;
using FR8.Sockets;
using UnityEngine;

namespace FR8.Level.Fuel
{
    [SelectionBase, DisallowMultipleComponent]
    public class FuelPumpHandle : PickupObject, ISocketable
    {
        public const float TargetSpring = 100.0f;
        public const float TargetDamping = 10.0f;
        public const float TargetTorqueScale = 1.0f;
        
        private SocketManager currentBinding;
        public string SocketType => "FuelPump";

        public bool CanBind() => currentBinding == null && !Held;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (currentBinding == null) return;

            var target = currentBinding.SocketTarget;
            transform.position = target.position;
            transform.rotation = target.rotation;
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

        public void LockRigidbody(bool state)
        {
            Rigidbody.isKinematic = state;
            Rigidbody.velocity = Vector3.zero;
        }

        public override PickupObject Pickup(PlayerGroundedAvatar target)
        {
            currentBinding?.Unbind();
            return base.Pickup(target);
        }
    }
}