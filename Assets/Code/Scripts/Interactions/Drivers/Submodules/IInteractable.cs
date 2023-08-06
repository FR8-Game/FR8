namespace FR8.Interactions.Drivers.Submodules
{
    public interface IInteractable : IBehaviour
    {
        bool CanInteract { get; }
        
        string DisplayName { get; }
        string DisplayValue { get; }
    }
}