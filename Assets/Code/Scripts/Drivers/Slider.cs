using UnityEngine;

namespace FR8.Drivers
{
    public class Slider : LinearDriver, IDriver
    {
        [Space]
        [SerializeField] private Transform handle;
        [SerializeField] private Vector3 slidePoseA;
        [SerializeField] private Vector3 slidePoseB;
        
        private Vector3 lastRayPoint;
        private float lastDragPosition;

        public override bool Limited => true;
        
        private Vector3 GetPointFromRay(Ray ray)
        {
            var normal = transform.up.normalized;
            var plane = new Plane(normal, transform.position);
            if (plane.Raycast(ray, out var enter))
            {
                lastRayPoint = ray.GetPoint(enter);
            }
            return lastRayPoint;
        }
        
        public override void OnBeginDrag(Ray ray)
        {
            var point = GetPointFromRay(ray);
            var v = point - transform.position;
            lastDragPosition = Vector3.Dot(transform.forward, v);
        }

        public override void OnContinueDrag(Ray ray, ref float value)
        {
            var point = GetPointFromRay(ray);
            var v = point - transform.position;
            var position = Vector3.Dot(transform.forward, v);
            var delta = position - lastDragPosition;
            lastDragPosition = position;

            var sensitivity = (slidePoseA - slidePoseB).magnitude;
            
            value += delta / sensitivity;
        }

        public override void DisplaySmoothedValue(float smoothedValue)
        {
            if (handle) handle.localPosition = Vector3.Lerp(slidePoseA, slidePoseB, smoothedValue);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!handle) handle = transform.GetChild(0);
        }
    }
}