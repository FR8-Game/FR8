using FR8.Interactions.Drivers;

namespace FR8.Interactions.Drivables
{
    public interface IDrivable : IBehaviour
    {
        void SetValue(DriverGroup group, float value);
    }
}