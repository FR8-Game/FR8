namespace FR8.Interactions.Drivers
{
    public interface IInteractable : IBehaviour
    {
        bool CanInteract { get; }
        
        string DisplayName { get; }
        string DisplayValue { get; }
    }
}