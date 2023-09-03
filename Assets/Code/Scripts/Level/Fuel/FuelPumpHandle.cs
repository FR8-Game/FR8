using FR8Runtime.Pickups;
using FR8Runtime.Player;
using FR8Runtime.Sockets;
using FR8Runtime.Train.Electrics;
using UnityEngine;

namespace FR8Runtime.Level.Fuel
{
    [SelectionBase, DisallowMultipleComponent]
    public class FuelPumpHandle : PickupObject, ISocketable
    {
        public static readonly CodeUtility.PhysicsUtility.SpringSettings SpringSettings = new()
        {
            spring = 300.0f,
            damper = 18.0f,
            torqueScale = 1.0f,
        };

        private ParticleSystem[] disconnectFX;
        private Collider[] colliders;

        public SocketManager CurrentBinding { get; private set; }
        public string SocketType => "Fuel";

        public bool CanBind() => !CurrentBinding && !Held;
        public TrainGasTurbine Engine { get; private set; }
        public override string DisplayValue => Engine ? $"{Engine.FuelLevel * 100.0f:N0}%" : base.DisplayValue;

        protected override void Awake()
        {
            base.Awake();
            colliders = GetComponentsInChildren<Collider>();
            disconnectFX = GetComponentsInChildren<ParticleSystem>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!CurrentBinding) return;

            var target = CurrentBinding.transform;
            CodeUtility.DampedSpring.ApplyForce(Rigidbody, target.position, target.rotation, SpringSettings.spring, SpringSettings.damper, SpringSettings.torqueScale, true);
        }

        public ISocketable Bind(SocketManager manager)
        {
            if (!CanBind()) return null;

            LockRigidbody(true);

            CurrentBinding = manager;
            Engine = manager.GetComponentInParent<TrainGasTurbine>();
            return this;
        }

        public ISocketable Unbind()
        {
            LockRigidbody(false);

            if (Engine) foreach (var p in disconnectFX) p.Play();
            
            CurrentBinding = null;
            Engine = null;
            return null;
        }

        public override PickupObject Pickup(PlayerAvatar target)
        {
            if (CurrentBinding) CurrentBinding.Unbind();
            return base.Pickup(target);
        }

        public void LockRigidbody(bool state)
        {
            foreach (var e in colliders) e.isTrigger = state;
        }
    }
}