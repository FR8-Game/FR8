using FR8.Pickups;
using FR8.Player.Submodules;
using FR8.Sockets;
using FR8.Train.Electrics;
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

        private Collider[] colliders;

        private SocketManager currentBinding;
        public string SocketType => "Fuel";

        public bool CanBind() => !currentBinding && !Held;
        public TrainGasTurbine Engine { get; private set; }
        public override string DisplayValue => Engine ? $"{Engine.FuelLevel * 100.0f:N0}%" : base.DisplayValue;

        protected override void Awake()
        {
            base.Awake();
            colliders = GetComponentsInChildren<Collider>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!currentBinding) return;

            var target = currentBinding.transform;
            Utility.DampedSpring.ApplyForce(Rigidbody, target.position, target.rotation, SpringSettings.spring, SpringSettings.damper, SpringSettings.torqueScale, true);
        }

        public ISocketable Bind(SocketManager manager)
        {
            if (!CanBind()) return null;

            LockRigidbody(true);

            currentBinding = manager;
            Engine = manager.GetComponentInParent<TrainGasTurbine>();
            return this;
        }

        public ISocketable Unbind()
        {
            LockRigidbody(false);

            currentBinding = null;
            Engine = null;
            return null;
        }

        public override PickupObject Pickup(PlayerInteractionManager target)
        {
            if (currentBinding) currentBinding.Unbind();
            return base.Pickup(target);
        }

        public void LockRigidbody(bool state)
        {
            foreach (var e in colliders) e.isTrigger = state;
        }
    }
}