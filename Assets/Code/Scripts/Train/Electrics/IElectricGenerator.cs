namespace FR8.Runtime.Train.Electrics
{
    public interface IElectricGenerator : IBehaviour
    {
        float MaximumPowerGeneration { get; }
        
        void SetClockSpeed(float percent);
    }
}