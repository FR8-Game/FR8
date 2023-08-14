namespace FR8.Interactions.Drivables
{
    public interface IDrivable : IBehaviour
    {
        string Key { get; }
        void OnValueChanged(float newValue);
    }
}