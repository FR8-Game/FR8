using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Train.Electrics;
using FR8Runtime.Train.Engine;
using UnityEngine;
using UnityEngine.Serialization;

namespace FR8Runtime.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrainEngine), typeof(TrainElectricsController), typeof(TrainGasTurbine))]
    [RequireComponent(typeof(DriverNetwork), typeof(TrainMonitor), typeof(LocomotiveAudio))]
    public class Locomotive : TrainCarriage
    {
        [FormerlySerializedAs("dynamicBrakeConstant")] [SerializeField] private float brakeConstant = 0.5f;
        [SerializeField] private Bounds localCabinBounds;

        [Space]
        [Header("Testing")]
        [SerializeField] private float initialVelocity;
        
        private float brakeLoad;

        private const string BrakeKey = "Brake";
        private const string GearKey = "Gear";
        private const string SpeedometerKey = "Speed";

        private float engineVelocity;
        private float enginePower;

        public float Brake => DriverNetwork.GetValue(BrakeKey);
        public int Gear => Mathf.RoundToInt(DriverNetwork.GetValue(GearKey));
        public Bounds LocalCabinBounds => localCabinBounds;

        protected override void Configure()
        {
            base.Configure();

            brakeConstant = Mathf.Max(0.0f, brakeConstant);
        }

        protected override void Start()
        {
            base.Start();
            
            DriverNetwork.SetValue(BrakeKey, 1.0f);
            DriverNetwork.SetValue(GearKey, 0.0f);
            
#if UNITY_EDITOR
            var t = segment.GetClosestPoint(Body.position, true);
            var dir = segment.SampleTangent(t);
            Body.AddForce(dir * initialVelocity / 3.6f, ForceMode.VelocityChange);
#endif
        }

        protected override void FixedUpdate()
        {
            ApplyBrake();
            base.FixedUpdate();

            UpdateDriverGroups();
        }

        private void ApplyBrake()
        {
            var fwdSpeed = GetForwardSpeed();

            var force = brakeConstant * Brake * -fwdSpeed;
            brakeLoad = force;

            var velocityChange = force * referenceWeight / Body.mass * Time.deltaTime;
            if (Mathf.Abs(velocityChange) > Mathf.Abs(fwdSpeed)) velocityChange = -fwdSpeed;

            Body.AddForce(DriverDirection * velocityChange, ForceMode.VelocityChange);
        }
        
        public void UpdateDriverGroups()
        {
            var fwdSpeed = Mathf.Abs(ToKmpH(GetForwardSpeed()));
            DriverNetwork.SetValue(SpeedometerKey, fwdSpeed);
        }

        protected override float GetBrakeLoad() => Mathf.Max(base.GetBrakeLoad(), brakeLoad);

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(localCabinBounds.center, localCabinBounds.size);
            Gizmos.color = Color.red.Alpha(0.2f);
            Gizmos.DrawCube(localCabinBounds.center, localCabinBounds.size);
        }
    }
}