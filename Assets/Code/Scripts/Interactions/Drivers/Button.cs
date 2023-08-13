using System;
using FR8.Interactions.Drivables;
using FR8.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    public class Button : Driver
    {
        [SerializeField] private TwoPoseDrivableAnimator animator;
        [SerializeField] private bool testValue;
        [SerializeField] private DriverSounds sounds;

        private bool state;

        public override string DisplayValue => state ? "Pressed" : "Press";

        public override void OnValueChanged(float newValue)
        {
            base.OnValueChanged(newValue);
            animator.SetValue(newValue);
            sounds.SetValue(newValue, 2);
        }

        protected override void Awake()
        {
            base.Awake();
            sounds.Awake(gameObject);
        }

        private void FixedUpdate()
        {
            SetValue(state ? 1.0f : 0.0f);
            state = false;
            
            animator.FixedUpdate();
        }

        private void Update()
        {
            animator.Update();
        }

        private void OnValidate()
        {
            animator.OnValidate(testValue ? 1.0f : 0.0f);
        }

        public override void Nudge(int direction) { }

        public override void BeginInteract(GameObject interactingObject) { }
        public override void ContinueInteract(GameObject interactingObject)
        {
            state = true;
        }
    }
}