using UnityEngine;

namespace FR8.Drivers
{
    public interface IInteractable : IBehaviour
    {
        bool CanInteract { get; }
        
        string DisplayName { get; }
        string DisplayValue { get; }
        
        void Nudge(int direction);
        void Press();

        void BeginDrag(Ray ray);
        void ContinueDrag(Ray ray);
    }
}