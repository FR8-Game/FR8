using TMPro;
using UnityEngine;

namespace FR8.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class LinearDriver : MonoBehaviour, IDriver
    {
        [SerializeField] protected string displayName = "Linear Driver";
        [SerializeField] private float nudgeStep = 8;
        [SerializeField] private float smoothTime = 0.06f;

        [Space]
        [SerializeField] protected Vector2 displayRange = new(0.0f, 100.0f);
        [SerializeField] protected string displayTemplate = "{0:N1}%";

        [Space]
        [SerializeField] protected TMP_Text displayText;

        protected DriverGroup driverGroup;

        private float internalValue;
        private float slidingValue;
        private float displayValue, velocity;

        public abstract bool Limited { get; }
        public bool CanInteract => true;
        public virtual string DisplayName => displayName;
        public virtual string DisplayValue => string.Format(displayTemplate, Mathf.LerpUnclamped(displayRange.x, displayRange.y, Value));
        public virtual float Value
        {
            get => driverGroup ? driverGroup.Value : internalValue;
            set
            {
                var v = Limited ? Mathf.Clamp01(value) : value;
                
                if (driverGroup) internalValue = driverGroup.SetValue(v);
                else
                {
                    internalValue = v;
                    ValueUpdated();
                }
            }
        }

        public void Nudge(int direction)
        {
            Value += direction * 1.0f / nudgeStep;
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