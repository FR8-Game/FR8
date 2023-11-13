using System;
using System.Collections.Generic;
using FR8.Runtime.Interactions.Drivers.Submodules;
using FR8.Runtime.Player;
using FR8.Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8.Runtime.Interactions
{
    public class Consumables : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName;
        [SerializeField] private string animationName;
        [SerializeField] private string verb;
        [SerializeField] private bool destroyOnConsumption;
        
        private IEnumerable<Renderer> visuals;
        
        public bool CanInteract => true;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string DisplayValue => verb;
        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new NotImplementedException();
        public IInteractable.InteractionType Type => IInteractable.InteractionType.Press;

        private void Awake()
        {
            visuals = GetComponentsInChildren<Renderer>();
        }

        public void Nudge(int direction) { }

        public void BeginInteract(GameObject interactingObject)
        {
            FpAnimations.Play(animationName);
            if (destroyOnConsumption) Destroy(gameObject);
        }

        public void ContinueInteract(GameObject interactingObject) { }
        public void Highlight(bool highlight)
        {
            if (highlight) SelectionOutlinePass.Add(visuals);
            else SelectionOutlinePass.Remove(visuals);
        }
    }
}
