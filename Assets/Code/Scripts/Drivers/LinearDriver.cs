using TMPro;
using UnityEngine;

namespace FR8.Drivers
{
    /// <summary>
    /// A base classed used to make a interactable control that provides a one dimensional, decimal value like a slider or a dial.
    /// </summary>
    [SelectionBase, DisallowMultipleComponent]
    public abstract class LinearDriver : MonoBehaviour, IDriver
    {
        // The Name Used when the player mouses over the object.
        [SerializeField] protected string displayName = "Linear Driver";
        // The smoothdamp time used to move the driver handle to the current driver value.
        [SerializeField] private float smoothTime = 0.06f;

        [Space]
        // The minimum and maximum values used when displaying the value of this driver.
        [SerializeField] protected Vector2 displayRange = new(0.0f, 100.0f);
        // The string template used when displaying the value.
        // The Value can be inserted with {0}
        // Uses C# Standard Numeric Format Strings - see https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        [SerializeField] protected string displayTemplate = "{0:N1}%";

        [Space]
        // An optional text object that will show the display value.
        [SerializeField] protected TMP_Text displayText;

        protected DriverGroup driverGroup;

        private float slidingValue;
        private float displayValue, velocity;

        public abstract bool Limited { get; }
        public bool CanInteract => true;
        public virtual string DisplayName => displayName;
        public virtual string DisplayValue => string.Format(displayTemplate, Mathf.LerpUnclamped(displayRange.x, displayRange.y, Value));
        public virtual float Value
        {
            get => driverGroup ? driverGroup.NormalizedValue : 0.0f;
            set
            {
                if (driverGroup) driverGroup.SetNormalizedValue(value);
            }
        }

        public void Nudge(int direction)
        {
            if (!driverGroup) return;
            driverGroup.Nudge(direction);
        }

        public virtual void Press() { }

        public void BeginDrag(Ray ray)
        {
            slidingValue = Value;
            OnBeginDrag(ray);
        }

        public void ContinueDrag(Ray ray)
        {
            OnContinueDrag(ray, ref slidingValue);
            Value = slidingValue;
        }

        public abstract void OnBeginDrag(Ray ray);
        public abstract void OnContinueDrag(Ray ray, ref float value);

        private void Update()
        {
            displayValue = Mathf.SmoothDamp(displayValue, Value, ref velocity, smoothTime);
            DisplaySmoothedValue(displayValue);
        }
        
        public virtual void ValueUpdated()
        {
            if (displayText) displayText.text = DisplayValue;
        }

        public virtual void DisplaySmoothedValue(float smoothedValue) { }

        public void SetDriverGroup(DriverGroup group)
        {
            driverGroup = group;
        }

        protected virtual void OnValidate()
        {
            ValueUpdated();
        }
    }
}