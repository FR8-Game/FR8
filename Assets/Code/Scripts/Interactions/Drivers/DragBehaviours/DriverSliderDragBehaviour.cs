using UnityEngine;

namespace FR8Runtime.Interactions.Drivers.DragBehaviours
{
    [System.Serializable]
    public sealed class DriverSliderDragBehaviour : DriverDragBehaviour
    {
        [SerializeField] private Vector3 dragVector = Vector3.forward;
        
        private Vector3 lastDragPosition;
        
        public override void BeginDrag(Transform transform, float value, Ray ray)
        {
            base.BeginDrag(transform, value, ray);
            lastDragPosition = GetDragPoint(transform, ray);
        }

        public override float ContinueDrag(Transform transform, Ray ray)
        {
            var dragPosition = GetDragPoint(transform, ray);
            var delta = dragPosition - lastDragPosition;
            var dragDirection = dragVector.normalized;
            
            var scalar = dragVector.magnitude * Sensitivity;
            Value += Vector3.Dot(dragDirection, delta) * scalar;
            
            lastDragPosition = dragPosition;
            return Value;
        }

        private Vector3 GetDragPoint(Transform transform, Ray ray)
        {
            var plane = GetDragPlane(transform, ray);
            if (plane.Raycast(ray, out var enter)) return transform.InverseTransformPoint(ray.GetPoint(enter));
            
            var direction = (ray.direction - plane.normal * Vector3.Dot(plane.normal, ray.direction)).normalized;
            return transform.InverseTransformPoint(ray.origin + direction * 1000.0f);
        }
        
        private Plane GetDragPlane(Transform transform,Ray ray)
        {
            var dragDirection = transform.TransformDirection(dragVector).normalized;
            var normal = -ray.direction.normalized;
            normal -= dragDirection * Vector3.Dot(dragDirection, normal);
            normal.Normalize();

            return new Plane(normal, transform.position);
        }
    }
}
