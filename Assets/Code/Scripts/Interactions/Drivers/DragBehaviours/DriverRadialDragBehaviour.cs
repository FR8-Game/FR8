using UnityEngine;

namespace FR8Runtime.Interactions.Drivers.DragBehaviours
{
    [System.Serializable]
    public class DriverRadialDragBehaviour : DriverDragBehaviour
    {
        private float lastDragPosition;
        private Vector3 lastRayPosition;

        public override void BeginDrag(Transform transform, float value, Ray ray)
        {
            base.BeginDrag(transform, value, ray);
            lastDragPosition = GetAngleFromDragPoint(transform, ray);
        }

        public override float ContinueDrag(Transform transform, Ray ray)
        {
            var angle = GetAngleFromDragPoint(transform, ray);
            var deltaAngle = Mathf.DeltaAngle(angle, lastDragPosition);
            lastDragPosition = angle;
            Value += deltaAngle / 360.0f * Sensitivity;
            return Value;
        }
        
        private float GetAngleFromDragPoint(Transform transform, Ray ray)
        {
            var point = GetPointFromRay(transform, ray);
            var v = point - transform.position;
            var dragPoint = new Vector2()
            {
                x = Vector3.Dot(transform.right, v),
                y = Vector3.Dot(transform.forward, v)
            };

            return Mathf.Atan2(dragPoint.y, dragPoint.x) * Mathf.Rad2Deg;
        }
        
        private Vector3 GetPointFromRay(Transform transform, Ray ray)
        {
            var plane = new Plane(transform.up, transform.position);
            if (!plane.Raycast(ray, out var enter)) return lastRayPosition;

            lastRayPosition = ray.GetPoint(enter);
            return lastRayPosition;
        }
    }
}