using FR8Runtime.Interactions.Drivables;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    public class BreakerSwitch : Switch
    {
        [SerializeField] private TwoPoseDrivableAnimator indicator;

        public override void OnValueChanged(float newValue)
        {
            base.OnValueChanged(newValue);
            indicator.SetValue(Value);
        }

        protected override void Update()
        {
            base.Update();
            indicator.Update();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            indicator.FixedUpdate();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            indicator.OnValidate(testValue ? 1.0f : 0.0f);
        }
    }
}