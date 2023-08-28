using FR8Runtime.Sockets;
using FR8Runtime.Train.Electrics;

namespace FR8Runtime.Level.Fuel
{
    public class FuelSocket : SocketManager
    {
        private TrainGasTurbine engine;

        public override string DisplayValue => $"{engine.FuelLevel * 100.0f:N0}%";

        protected override void Awake()
        {
            base.Awake();
            engine = GetComponentInParent<TrainGasTurbine>();
        }
    }
}