using UnityEngine;

namespace FR8.Interactions
{
    public class Slider : LinearDriver, IDriver
    {
        [Space]
        [SerializeField] private Transform handle;
        [SerializeField] private Vector3 slidePoseA;
        [SerializeField] private Vector3 slidePoseB;
        
        public override bool Limited => true;
        private Vector3 lastRayPoint;
        private float lastDragPosition;

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

        protected override void ValidateValue()
        {
            value = Mathf.Clamp01(value);
            base.ValidateValue();
        }

        public override void BeginDrag(Ray ray)
        {
            var point = GetPointFromRay(ray);
            var v = point - transform.position;
            lastDragPosition = Vector3.Dot(transform.forward, v);
        }

        public override void ContinueDrag(Ray ray)
        {
            var point = GetPointFromRay(ray);
            var v = point - transform.position;
            var position = Vector3.Dot(transform.forward, v);
            var delta = position - lastDragPosition;
            lastDragPosition = position;

            var sensitivity = (slidePoseA - slidePoseB).magnitude;
            
            value += delta / sensitivity;
            UpdateVisuals();
        }

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            if (handle) handle.localPosition = Vector3.Lerp(slidePoseA, slidePoseB, Output);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!handle) handle = transform.GetChild(0);
        }
    }
}