using UnityEngine;

namespace FR8.Drivers.DragBehaviours
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public abstract class DriverDragBehaviour : MonoBehaviour
    {
        [SerializeField] private float sensitivity;

        public float Value { get; protected set; }
        public float Sensitivity => sensitivity;

        public abstract void BeginDrag(Ray ray);
        public abstract float ContinueDrag(Ray ray);
    }
}
