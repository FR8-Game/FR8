using FR8.Train;
using FR8.Train.Electrics;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    public class ExternalTrainDoor : Door, IElectricDevice
    {
        private const float MovementThreshold = 5.0f;
        
        private bool connected;
        private bool traveling;
        private TrainCarriage trainMovement;
        
        public string FuseGroup => null;

        public bool Locked => traveling || connected;
        public override bool CanInteract => !Locked;
        public override string DisplayValue
        {
            get
            {
                if (traveling) return "Locked [Train is in Motion]";
                if (connected) return "Locked [Train is Powered]";
                return base.DisplayValue;
            }
        }

        public void SetConnected(bool connected)
        {
            this.connected = connected;
            if (Locked) SetValue(0.0f);
        }

        protected override void Awake()
        {
            base.Awake();
            trainMovement = GetComponentInParent<TrainCarriage>();
        }

        protected override void FixedUpdate()
        {
            var fwdSpeed = trainMovement.GetForwardSpeed();
            traveling = Mathf.Abs(fwdSpeed) > MovementThreshold;
            if (Locked) SetValue(0.0f);
            
            base.FixedUpdate();
        }

        protected override void SetValue(float newValue)
        {
            base.SetValue(Locked ? 0.0f : newValue);
        }

        public float CalculatePowerDraw() => 0.0f;
    }
}