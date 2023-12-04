using FR8.Runtime.Interactions.Drivers;
using FR8.Runtime.Train.Electrics;
using FR8.Runtime.Train.Engine;
using UnityEngine;

namespace FR8.Runtime.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrainEngine), typeof(TrainElectricsController), typeof(TrainGasTurbine))]
    [RequireComponent(typeof(DriverNetwork), typeof(LocomotiveAudio))]
    public class Locomotive : TrainCarriage
    {
        public LocomotiveSettings locomotiveSettings;
        [SerializeField] private Bounds localCabinBounds;

        private float brakeLoad;

        private const string BrakeKey = "Brake";
        private const string GearKey = "Gear";
        private const string SpeedometerKey = "Speed";

        private float engineVelocity;
        private float enginePower;

        public float Brake => DriverNetwork.GetValue(BrakeKey);
        public int Gear => Mathf.RoundToInt(DriverNetwork.GetValue(GearKey));
        public Bounds LocalCabinBounds => localCabinBounds;

        protected override void Start()
        {
            base.Start();
            
            DriverNetwork.SetValue(BrakeKey, 1.0f);
            DriverNetwork.SetValue(GearKey, 0.0f);
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

            var force = locomotiveSettings.handbrakeConstant * Brake * -fwdSpeed;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(localCabinBounds.center, localCabinBounds.size);
            Gizmos.color = Color.red.Alpha(0.2f);
            Gizmos.DrawCube(localCabinBounds.center, localCabinBounds.size);
        }
    }
}