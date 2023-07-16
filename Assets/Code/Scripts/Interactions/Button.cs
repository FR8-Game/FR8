using System.Collections.Generic;
using FR8.Signals;
using UnityEngine;

namespace FR8.Interactions
{
    [SelectionBase, DisallowMultipleComponent]
    public class Button : MonoBehaviour, IDriver
    {
        [SerializeField] private string displayName = "Button";
        [SerializeField] private List<Signal> pressSignalList;

        public float Output => 0.0f;
        public virtual bool CanInteract => true;
        public bool Limited => true;
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