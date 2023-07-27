using UnityEngine;

namespace FR8.Interactions.Drivers.DragBehaviours
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public abstract class DriverDragBehaviour : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 1.0f;

        public float Value { get; protected set; }
        public float Sensitivity => sensitivity;

        public virtual void BeginDrag(float value, Ray ray)
        {
            Value = value;
        }
        public abstract float ContinueDrag(Ray ray);
    }
}
