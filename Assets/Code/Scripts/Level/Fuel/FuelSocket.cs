
using FR8.Sockets;
using FR8.Train.Electrics;

namespace FR8.Level.Fuel
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