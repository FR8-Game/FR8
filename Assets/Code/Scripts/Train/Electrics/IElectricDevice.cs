namespace FR8.Runtime.Train.Electrics
{
    public interface IElectricDevice : IBehaviour
    {
        float CalculatePowerDraw();
    }
}