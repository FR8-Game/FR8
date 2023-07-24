using UnityEngine;

namespace FR8.Drivers.DragBehaviours
{
    [DisallowMultipleComponent]
    public class DriverRadialDragBehaviour : DriverDragBehaviour
    {
        private float lastDragPosition;
        private Vector3 lastRayPosition;

        public override void BeginDrag(float value, Ray ray)
        {
            base.BeginDrag(value, ray);
            lastDragPosition = GetAngleFromDragPoint(ray);
        }

        public override float ContinueDrag(Ray ray)
        {
            var angle = GetAngleFromDragPoint(ray);
            var deltaAngle = Mathf.DeltaAngle(angle, lastDragPosition);
            lastDragPosition = angle;
            Value += deltaAngle / 360.0f * Sensitivity;
            return Value;
        }
        
        private float GetAngleFromDragPoint(Ray ray)
        {
            var point = GetPointFromRay(ray);
            var v = point - transform.position;
            var dragPoint = new Vector2()
            {
                x = Vector3.Dot(transform.right, v),
                y = Vector3.Dot(transform.forward, v)
            };

            return Mathf.Atan2(dragPoint.y, dragPoint.x) * Mathf.Rad2Deg;
        }
        
        private Vector3 GetPointFromRay(Ray ray)
        {
            var plane = new Plane(transform.up, transform.position);
            if (!plane.Raycast(ray, out var enter)) return lastRayPosition;

            lastRayPosition = ray.GetPoint(enter);
            return lastRayPosition;
        }
    }
}