using FR8.Drivers;

namespace FR8.Drivables
{
    public interface IDrivable : IBehaviour
    {
        void SetValue(DriverGroup group, float value);
    }
}