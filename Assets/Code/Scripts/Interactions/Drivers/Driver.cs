using FR8.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Driver : MonoBehaviour, IDriver
    {
        [SerializeField] private string key;
        [SerializeField] private string displayName;
        [SerializeField] private float defaultValue;
        
        private DriverNetwork driverNetwork;
        
        public virtual bool CanInteract => true;
        public virtual string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public virtual string DisplayValue => Mathf.RoundToInt(Value * 100.0f).ToString();
        
        public float Value { get; private set; }
        public string Key => key;
        
        public virtual void OnValueChanged(float newValue)
        {
            Value = newValue;
        }

        protected virtual void SetValue(float newValue)
        {
            driverNetwork.SetValue(key, newValue);
        }

        public abstract void Nudge(int direction);

        public abstract void BeginDrag(Ray ray);

        public abstract void ContinueDrag(Ray ray);

        protected virtual void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();
        }

        protected void Start()
        {
            SetValue(defaultValue);
        }
    }
}