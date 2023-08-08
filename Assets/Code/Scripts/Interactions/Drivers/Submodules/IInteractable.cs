using UnityEngine;

namespace FR8.Interactions.Drivers.Submodules
{
    public interface IInteractable : IBehaviour
    {
        bool CanInteract { get; }
        
        string DisplayName { get; }
        string DisplayValue { get; }
        
        bool OverrideInteractDistance { get; }
        float InteractDistance { get; }
        
        void Nudge(int direction);
        
        void BeginDrag(Ray ray);
        void ContinueDrag(Ray ray);
    }
}