namespace FR8.Train.Electrics
{
    public interface IElectricGenerator : IBehaviour
    {
        float MaximumPowerGeneration { get; }
        
        void SetClockSpeed(float percent);
        void ChangedFuseState(bool fuseState);
    }
}