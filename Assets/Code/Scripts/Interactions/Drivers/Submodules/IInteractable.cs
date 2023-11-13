using UnityEngine;

namespace FR8.Runtime.Interactions.Drivers.Submodules
{
    public interface IInteractable : IBehaviour
    {
        bool CanInteract { get; }
        
        string DisplayName { get; }
        string DisplayValue { get; }
        InteractionType Type { get; }
        
        bool OverrideInteractDistance { get; }
        float InteractDistance { get; }

        void Nudge(int direction);
        
        void BeginInteract(GameObject interactingObject);
        void ContinueInteract(GameObject interactingObject);

        void Highlight(bool highlight);

        public enum InteractionType
        {
            Scroll,
            Press,
        }
    }
}