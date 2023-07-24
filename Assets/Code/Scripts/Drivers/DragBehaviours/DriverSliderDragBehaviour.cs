using UnityEngine;

namespace FR8.Drivers.DragBehaviours
{
    [DisallowMultipleComponent]
    public sealed class DriverSliderDragBehaviour : DriverDragBehaviour
    {
        [SerializeField] private Vector3 dragVector = Vector3.forward;
        
        private Vector3 lastDragPosition;
        
        public override void BeginDrag(float value, Ray ray)
        {
            base.BeginDrag(value, ray);
            lastDragPosition = GetDragPoint(ray);
        }

        public override float ContinueDrag(Ray ray)
        {
            var dragPosition = GetDragPoint(ray);
            var delta = dragPosition - lastDragPosition;
            var dragDirection = dragVector.normalized;
            
            var scalar = dragVector.magnitude * Sensitivity;
            Value += Vector3.Dot(dragDirection, delta) * scalar;
            
            lastDragPosition = dragPosition;
            return Value;
        }

        private Vector3 GetDragPoint(Ray ray)
        {
            var plane = GetDragPlane(ray);
            if (!plane.Raycast(ray, out var enter)) return default;

            return transform.InverseTransformPoint(ray.GetPoint(enter));
        }
        
        private Plane GetDragPlane(Ray ray)
        {
            var dragDirection = transform.TransformDirection(dragVector).normalized;
            var normal = -ray.direction.normalized;
            normal -= dragDirection * Vector3.Dot(dragDirection, normal);
            normal.Normalize();

            return new Plane(normal, transform.position);
        }
    }
}
