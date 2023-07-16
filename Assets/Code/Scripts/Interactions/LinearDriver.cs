using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace FR8.Interactions
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class LinearDriver : MonoBehaviour, IDriver
    {
        [SerializeField] protected string displayName = "Linear Driver";

        [Space]
        [SerializeField] protected float defaultValue = 0.0f;
        [SerializeField] protected int step = 10;
        [SerializeField] protected bool forceStep = true;
        [SerializeField] protected Vector2 displayRange = new(0.0f, 100.0f);
        [SerializeField] protected string displayTemplate = "{0:N1}%";

        [Space]
        [SerializeField] protected TMP_Text displayText;

        [Space]
        [SerializeField] private Vector2 dragDirection = Vector2.up;
        [SerializeField] private float dragSensitivity = 1.0f;
        
        [Space]
        [SerializeField] protected UnityEvent<float> OnValueChanged;
        [SerializeField] protected UnityEvent<string> OnDisplayChanged;

        protected float value;

        public float Output
        {
            get => forceStep ? Mathf.Round(value * step) / step : value;
            set => this.value = value;
        }

        public bool CanInteract => true;
        public abstract bool Limited { get; }
        public virtual string DisplayName => displayName;
        public virtual string DisplayValue => string.Format(displayTemplate, Mathf.LerpUnclamped(displayRange.x, displayRange.y, Output));

        private void Start()
        {
            value = defaultValue;
            UpdateVisuals();
        }

        public void Nudge(int direction)
        {
            value += direction * 1.0f / step;
            UpdateVisuals();
        }

        protected virtual void ValidateValue() { }

        public virtual void Press() { }
        
        public abstract void BeginDrag(Ray ray);
        public abstract void ContinueDrag(Ray ray);

        protected virtual void UpdateVisuals()
        {
            ValidateValue();
            OnValueChanged?.Invoke(Output);
            OnDisplayChanged?.Invoke(DisplayValue);
            if (displayText) displayText.text = DisplayValue;
        }

        protected virtual void OnValidate()
        {
            value = defaultValue;
            UpdateVisuals();
        }
    }
}