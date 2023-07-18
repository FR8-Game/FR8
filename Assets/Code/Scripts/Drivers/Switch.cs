using UnityEngine;

namespace FR8.Drivers
{
    public class Switch : Slider
    {
        [Space]
        [SerializeField] private string trueDisplay = "On";
        [SerializeField] private string falseDisplay = "Off";

        public override string DisplayValue => State ? trueDisplay : falseDisplay;
        public bool State => Value > 0.5f;

        public override float Value
        {
            get => base.Value > 0.5f ? 1.0f : 0.0f;
            set => base.Value = value > 0.5f ? 1.0f : 0.0f;
        }

        public override void Press()
        {
            Value = 1.0f - Value;
        }

        public override void BeginDrag(Ray ray) { }
        public override void ContinueDrag(Ray ray) { }
    }
}