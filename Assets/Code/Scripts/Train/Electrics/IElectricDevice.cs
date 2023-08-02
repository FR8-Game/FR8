namespace FR8.Train.Electrics
{
    public interface IElectricDevice : IBehaviour
    {
        void SetConnected(bool connected);
        float CalculatePowerDraw();
    }
}