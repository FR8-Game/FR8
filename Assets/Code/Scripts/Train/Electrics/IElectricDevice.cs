namespace FR8.Train.Electrics
{
    public interface IElectricDevice : IBehaviour
    {
        string FuseGroup { get; }
        
        void SetConnected(bool connected);
        float CalculatePowerDraw();
    }
}