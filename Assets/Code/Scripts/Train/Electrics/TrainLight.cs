using FMOD.Studio;
using FMODUnity;
using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Level;
using FR8Runtime.References;
using UnityEngine;

namespace FR8Runtime.Train.Electrics
{
    public class TrainLight : LightDriver, IElectricDevice
    {
        [SerializeField] private string fuseGroup = "Lights";
        [SerializeField] private float powerDrawWatts = 40.0f;
        [SerializeField] private float threshold = 0.5f;
        
        private EventInstance soundInstance;
        private DriverNetwork driverNetwork;

        private bool wasOn;
        
        public string FuseGroup => fuseGroup;

        protected override void Awake()
        {
            base.Awake();
            
            soundInstance = SoundReference.LightHum.Instance();
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        protected override void FixedUpdate()
        {
            var switchState = driverNetwork.GetValue(fuseGroup) > threshold;
            var fuseState = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f;

            wasOn = state;
            state = fuseState && switchState;

            if (state != wasOn)
            {
                if (state) soundInstance.start();
                else soundInstance.stop(STOP_MODE.ALLOWFADEOUT);
            }

            if (state) soundInstance.set3DAttributes(gameObject.To3DAttributes());

            base.FixedUpdate();
        }

        public float CalculatePowerDraw() => state ? powerDrawWatts / 1000.0f : 0.0f;
    }
}
