using FMOD.Studio;
using FMODUnity;
using FR8.Runtime.Interactions.Drivers;
using FR8.Runtime.Level;
using FR8.Runtime.References;
using UnityEngine;

namespace FR8.Runtime.Train.Electrics
{
    public class TrainLight : LightDriver, IElectricDevice
    {
        private const float LoadFlickerFrequency = 15.0f;
        private const float LoadFlickerAmplitude = 0.15f;
        
        [SerializeField] private string fuseGroup = "Lights";
        [SerializeField] private float powerDrawWatts = 40.0f;
        [SerializeField] private float driverValueThreshold = 0.5f;
        
        [Space]
        [SerializeField] private AnimationCurve loadFlickerCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        
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
            var switchState = driverNetwork.GetValue(fuseGroup) > driverValueThreshold;
            var fuseState = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f;

            wasOn = State;
            State = fuseState && switchState;

            if (State != wasOn)
            {
                if (State) soundInstance.start();
                else soundInstance.stop(STOP_MODE.ALLOWFADEOUT);
            }

            if (State) soundInstance.set3DAttributes(gameObject.To3DAttributes());

            base.FixedUpdate();
        }

        protected override float CalculateAttenuation()
        {
            if (!driverNetwork) return base.CalculateAttenuation();
            
            var load = driverNetwork.GetValue("load");
            var flicker = ColorFlickerinator.Flicker(LoadFlickerFrequency, LoadFlickerAmplitude);
            
            return Mathf.Lerp(base.CalculateAttenuation(), flicker, loadFlickerCurve.Evaluate(load));
        }

        public float CalculatePowerDraw() => State ? powerDrawWatts / 1000.0f : 0.0f;
    }
}
