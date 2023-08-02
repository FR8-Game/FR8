
using FR8.Interactions.Drivables;
using FR8.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    public class Switch : Driver
    {
        [SerializeField] protected bool testValue;
        
        [Space]
        [SerializeField] private TwoPoseDrivableAnimator animator;
        [SerializeField] private DriverSounds sounds;

        public override void OnValueChanged(float newValue)
        {
            animator.SetValue(newValue);
            sounds.SetValue(newValue, 2);
            
            base.OnValueChanged(newValue);
        }

        protected override void SetValue(float newValue)
        {
            newValue = newValue > 0.5f ? 1.0f : 0.0f;
            base.SetValue(newValue);
        }

        public override void Nudge(int direction)
        {
            SetValue(direction);
        }

        public override void BeginDrag(Ray ray) => SetValue(1.0f - Value);

        public override void ContinueDrag(Ray ray) { }

        protected override void Awake()
        {
            base.Awake();
            
            sounds.Awake(gameObject);
        }

        protected virtual void Update()
        {
            animator.Update();
        }

        protected virtual void FixedUpdate()
        {
            animator.FixedUpdate();
        }

        protected virtual void OnValidate()
        {
            animator.OnValidate(testValue ? 1.0f : 0.0f);
        }
    }
}