using System.Collections.Generic;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers.Submodules
{
    public interface IInteractable : IBehaviour
    {
        bool CanInteract { get; }
        
        string DisplayName { get; }
        string DisplayValue { get; }
        
        bool OverrideInteractDistance { get; }
        float InteractDistance { get; }
        
        IEnumerable<Renderer> Visuals { get; }

        void Nudge(int direction);
        
        void BeginInteract(GameObject interactingObject);
        void ContinueInteract(GameObject interactingObject);
    }
}