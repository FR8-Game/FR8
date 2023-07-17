namespace FR8.Drivers
{
    public interface IDriver : IInteractable
    {
        void ValueUpdated();
        void SetDriverGroup(DriverGroup group);
    }
}