namespace FR8Runtime.Train.Electrics
{
    public interface IElectricDevice : IBehaviour
    {
        float CalculatePowerDraw();
    }
}