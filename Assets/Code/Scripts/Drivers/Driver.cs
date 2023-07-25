using FR8.Drivers.DragBehaviours;
using UnityEngine;

namespace FR8.Drivers
{
    /// <summary>
    /// A base classed used to make a interactable control that provides a one dimensional, decimal value like a slider or a dial.
    /// </summary>
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Driver : MonoBehaviour, IDriver
    {
        // The Name Used when the player mouses over the object.
        [SerializeField] protected float initialValue;

        protected DriverGroup driverGroup;
        protected DriverDragBehaviour dragBehaviour;

        public virtual bool CanInteract => true;
        public virtual string DisplayName => driverGroup ? driverGroup.GroupName : name;
        public abstract string DisplayValue { get; }
        public virtual float Value
        {
            get => driverGroup ? driverGroup.Value : 0.0f;
            set
            {
                if (driverGroup) driverGroup.SetValue(value);
            }
        }

        protected virtual void Awake()
        {
            dragBehaviour = GetComponent<DriverDragBehaviour>();
        }

        protected virtual void Start()
        {
            Value = initialValue;
        }

        public virtual void Nudge(int direction) { }

        public virtual void Press() { }

        public void BeginDrag(Ray ray)
        {
            if (dragBehaviour) dragBehaviour.BeginDrag(Value, ray);
        }

        public void ContinueDrag(Ray ray)
        {
            if (dragBehaviour) Value = dragBehaviour.ContinueDrag(ray);
        }

        public virtual void ValueUpdated() { }

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