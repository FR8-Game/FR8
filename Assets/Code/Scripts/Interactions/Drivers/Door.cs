using FR8.Interactions.Drivables;
using FR8.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class Door : MonoBehaviour, IDriver
    {
        [SerializeField] private bool testValue;
        [SerializeField] private TwoPoseDrivableAnimator animator;

        private bool state;

        public string Key => string.Empty;
        public virtual bool CanInteract => true;
        public string DisplayName => "Door";
        public virtual string DisplayValue => state ? "Open" : "Closed";

        protected virtual void Awake()
        {
            SetValue(testValue ? 1.0f : 0.0f);
        }

        public void OnValueChanged(float newValue) { }

        protected virtual void SetValue(float newValue)
        {
            state = newValue > 0.5f;
            animator.SetValue(state ? 1.0f : 0.0f);
        }

        public void Nudge(int direction)
        {
            SetValue(direction);
        }

        public void BeginDrag(Ray ray)
        {
            SetValue(state ? 0.0f : 1.0f);
        }

        public void ContinueDrag(Ray ray)
        {
        }

        private void Update()
        {
            animator.Update();
        }

        protected virtual void FixedUpdate()
        {
            animator.FixedUpdate();
        }

        private void OnValidate()
        {
            animator.OnValidate(testValue ? 1.0f : 0.0f);
        }
    }
}