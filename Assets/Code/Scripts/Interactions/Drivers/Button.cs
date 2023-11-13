using FR8.Runtime.Interactions.Drivables;
using FR8.Runtime.References;
using UnityEngine;

namespace FR8.Runtime.Interactions.Drivers
{
    public class Button : Driver
    {
        [SerializeField] private TwoPoseDrivableAnimator animator;
        [SerializeField] private bool testValue;

        private bool state;

        public override string DisplayValue => state ? "Pressed" : "Press";

        public override void OnValueChanged(float newValue)
        {
            base.OnValueChanged(newValue);
            animator.SetValue(newValue);

            SoundReference.ButtonPress.PlayOneShot();        
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
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