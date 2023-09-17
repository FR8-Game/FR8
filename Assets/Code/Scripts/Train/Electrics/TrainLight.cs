using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Level;
using UnityEngine;

namespace FR8Runtime.Train.Electrics
{
    public class TrainLight : LightDriver, IElectricDevice
    {
        [SerializeField] private string fuseGroup = "Lights";
        [SerializeField] private float powerDrawWatts = 40.0f;
        [SerializeField] private float threshold = 0.5f;
        
        private DriverNetwork driverNetwork;
        
        public string FuseGroup => fuseGroup;

        protected override void Awake()
        {
            base.Awake();

            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        protected override void FixedUpdate()
        {
            var switchState = driverNetwork.GetValue(fuseGroup) > threshold;
            var fuseState = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f;
            
            state = fuseState && switchState;
            base.FixedUpdate();
        }

        public float CalculatePowerDraw() => state ? powerDrawWatts / 1000.0f : 0.0f;
    }
}
