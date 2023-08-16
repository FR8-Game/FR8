using FR8.Interactions.Drivables;
using FR8.Level;
using UnityEngine;

namespace FR8.Train.Electrics
{
    public class TrainLight : LightDriver, IElectricDevice, IDrivable
    {
        [SerializeField] private string key;
        [SerializeField] private string fuseGroup = "Lights";
        [SerializeField] private float powerDrawWatts = 40.0f;
        [SerializeField] private float threshold = 0.5f;
        
        private bool driverState;
        private bool connected;

        public string Key => key;
        public string FuseGroup => fuseGroup;

        public void SetConnected(bool connected)
        {
            this.connected = connected;
        }

        protected override void FixedUpdate()
        {
            state = connected && driverState;
            base.FixedUpdate();
        }

        public float CalculatePowerDraw() => state ? powerDrawWatts / 1000.0f : 0.0f;
        public void OnValueChanged(float newValue) => driverState = newValue > threshold;
    }
}
