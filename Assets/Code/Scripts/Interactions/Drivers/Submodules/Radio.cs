using FR8.Dialogue;
using UnityEngine;

namespace FR8.Interactions.Drivers.Submodules
{
    public class Radio : Button
    {
        [SerializeField] private DialogueChain chain;
        
        public override string DisplayValue => CanInteract ? "Listen" : "Wait";
        public override bool CanInteract => !DialogueListener.IsDialogueListenerActive;

        public override void Press()
        {
            base.Press();
            chain.Queue();
        }
    }
}