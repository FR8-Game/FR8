using System;
using System.Collections.Generic;
using FR8.Interactions;
using FR8.Signals;
using UnityEngine;

namespace FR8
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class Button : MonoBehaviour, IDriver
    {
        [SerializeField] private string displayName = "Button";
        [SerializeField] private List<Signal> pressSignalList;

        public float Output => 0.0f;

        public bool Limited => true;
        public string DisplayName => displayName;
        public string DisplayValue => "Press";

        public void Nudge(int direction)
        {
            Press();
        }

        public void Press()
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