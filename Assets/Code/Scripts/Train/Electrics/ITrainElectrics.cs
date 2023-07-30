using UnityEngine;

namespace FR8.Train.Electrics
{
    [SelectionBase]
    public abstract class TrainElectrics : MonoBehaviour
    {
        [SerializeField] private int fuzeGroup;

        private bool connected;

        protected TrainElectricsController Controller { get; private set; }
        public int FuzeGroup => fuzeGroup;
        public bool Connected
        {
            get => connected;
            set
            {
                switch (value)
                {
                    case true when !connected:
                    {
                        connected = true;
                        OnConnected();
                        break;
                    }
                    case false when connected:
                    {
                        connected = false;
                        OnDisconnected();
                        break;
                    }
                }
            }
        }


        public virtual TrainElectrics SetController(TrainElectricsController controller)
        {
            this.Controller = controller;
            return this;
        }
        
        public abstract float CalculatePowerConsumptionMegawatts();

        public virtual void OnConnected() { }
        public virtual void OnDisconnected() { }
    }
}
