using UnityEngine;

namespace FR8.Interactions
{
    public class Switch : Slider
    {
        [Space]
        [SerializeField] private string trueDisplay = "On";
        [SerializeField] private string falseDisplay = "Off";

        public override string DisplayValue => State ? trueDisplay : falseDisplay;
        public bool State => Output > 0.5f;

        protected override void ValidateValue()
        {
            base.ValidateValue();
            Output = State ? 1.0f : 0.0f;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            step = 1;
        }

        public override void Press()
        {
            Output = 1.0f - Output;
            UpdateVisuals();
        }
    }
}