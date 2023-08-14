using System;
using FR8.Interactions.Drivers.Submodules;
using FR8.Player;
using UnityEngine;

namespace FR8.Interactions
{
    public class Consumables : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName;
        [SerializeField] private string animationName;
        [SerializeField] private string verb;
        [SerializeField] private bool destroyOnConsumption;
        
        public bool CanInteract => true;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string DisplayValue => verb;
        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new NotImplementedException();
        
        public void Nudge(int direction) { }

        public void BeginInteract(GameObject interactingObject)
        {
            FpAnimations.Play(animationName);
            if (destroyOnConsumption) Destroy(gameObject);
        }

        public void ContinueInteract(GameObject interactingObject) { }
    }
}
