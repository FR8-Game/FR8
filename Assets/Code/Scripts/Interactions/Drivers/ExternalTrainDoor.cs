using FR8Runtime.Train;

namespace FR8Runtime.Interactions.Drivers
{
    public class ExternalTrainDoor : Door
    {
        private bool traveling;
        private TrainCarriage trainMovement;
        
        public string FuseGroup => null;

        public bool Locked => traveling;
        public override bool CanInteract => !Locked;
        public override string DisplayValue
        {
            get
            {
                if (traveling) return "Locked [Train is in Motion]";
                return base.DisplayValue;
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            trainMovement = GetComponentInParent<TrainCarriage>();
        }

        protected override void FixedUpdate()
        {
            var fwdSpeed = trainMovement.GetForwardSpeed();
            //traveling = Mathf.Abs(fwdSpeed) > TrainMonitor.MaxTrainSafeSpeed;
            if (Locked) SetValue(0.0f);
            
            base.FixedUpdate();
        }

        protected override void SetValue(float newValue)
        {
            base.SetValue(Locked ? 0.0f : newValue);
        }
    }
}