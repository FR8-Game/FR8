using UnityEngine;

namespace FR8.Drivers
{
    public class LinearDriver : Driver
    {
        [Space]
        // The minimum and maximum values used when displaying the value of this driver.
        [SerializeField] protected Vector2 displayRange = new(0.0f, 100.0f);
        // The string template used when displaying the value.
        // The Value can be inserted with {0}
        // Uses C# Standard Numeric Format Strings - see https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        [SerializeField] protected string displayTemplate = "{0:N0}%";
        
        [Space]
        [SerializeField] private bool clamped = true;
        [SerializeField] private int steps = 10;
        [SerializeField] private bool forceStep;
        [SerializeField] private Vector2 valueRange = Vector2.up;
        
        public override string DisplayValue => string.Format(displayTemplate, Mathf.LerpUnclamped(displayRange.x, displayRange.y, Value));

        public override float Value
        {
            get => base.Value;
            set
            {
                var v = value;
                if (forceStep && steps > 0)
                {
                    var p = Mathf.InverseLerp(valueRange.x, valueRange.y, v);
                    p = Mathf.Round(p * steps) / steps;
                    v = Mathf.Lerp(valueRange.x, valueRange.y, p);
                }

                if (clamped)
                {
                    v = Mathf.Clamp(v, valueRange.x, valueRange.y);
                }
                
                base.Value = v;
            }
        }

        public override void Nudge(int direction)
        {
            var range = valueRange.y - valueRange.x;
            Value += direction * range / steps;
        }
    }
}