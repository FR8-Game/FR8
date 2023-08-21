namespace FR8.Train.Electrics
{
    public interface IElectricDevice : IBehaviour
    {
        float CalculatePowerDraw();
    }
}