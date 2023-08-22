using FR8Runtime.Interactions.Drivables;
using FR8Runtime.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class Door : MonoBehaviour, IDriver
    {
        [SerializeField] private bool testValue;
        [SerializeField] private TwoPoseDrivableAnimator animator;

        public string Key => string.Empty;
        public virtual bool CanInteract => true;
        public string DisplayName => "Door";
        public virtual string DisplayValue => Open ? "Open" : "Closed";
        public bool Open { get; private set; }

        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new System.NotImplementedException();
        
        protected virtual void Awake()
        {
            SetValue(testValue ? 1.0f : 0.0f);
        }

        public void OnValueChanged(float newValue) { }

        protected virtual void SetValue(float newValue)
        {
            Open = newValue > 0.5f;
            animator.SetValue(Open ? 1.0f : 0.0f);
        }

        public void Nudge(int direction)
        {
            SetValue(direction);
        }

        public void BeginInteract(GameObject interactingObject)
        {
            SetValue(Open ? 0.0f : 1.0f);
        }

        public void ContinueInteract(GameObject interactingObject)
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