using System.Collections.Generic;
using FR8.Train.Signals;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class Button : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Button";
        [SerializeField] private List<Signal> pressSignalList;

        public virtual bool CanInteract => true;
        public virtual string DisplayName => displayName;
        public virtual string DisplayValue => "Press";
        
        public void Nudge(int direction)
        {
            Press();
        }

        public virtual void Press()
        {
            pressSignalList.Raise();
        }

        public void BeginDrag(Ray ray)
        {
        }

        public void ContinueDrag(Ray ray)
        {
        }
    }
}