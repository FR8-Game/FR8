using UnityEngine;

namespace FR8Runtime.Interactions.Drivers.DragBehaviours
{
    [System.Serializable]
    public abstract class DriverDragBehaviour
    {
        [SerializeField] private float sensitivity = 1.0f;

        public float Value { get; protected set; }
        public float Sensitivity => sensitivity;

        public virtual void BeginDrag(Transform transform, float value, Ray ray)
        {
            Value = value;
        }
        public abstract float ContinueDrag(Transform transform, Ray ray);
    }
}
