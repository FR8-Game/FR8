using System;
using UnityEngine;

namespace FR8.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class Dial : LinearDriver
    {
        [Space]
        [SerializeField] private bool limited;
        
        [Space]
        [SerializeField] private DragMode dragMode = DragMode.Dial;

        [SerializeField] private Transform handle;
        [SerializeField] private Vector3 zeroRotation;
        [SerializeField] private Vector3 oneRotation;

        private float lastDragPosition;
        private Vector3 lastRayPosition;

        public override bool Limited => limited;

        private Vector3 GetPointFromRay(Ray ray)
        {
            var plane = new Plane(transform.up, transform.position);
            if (!plane.Raycast(ray, out var enter)) return lastRayPosition;

            lastRayPosition = ray.GetPoint(enter);
            return lastRayPosition;
        }
        
        private float GetAngleFromDragPoint(Ray ray)
        {
            var point = GetPointFromRay(ray);
            var v = point - handle.position;
            var dragPoint = new Vector2()
            {
                x = Vector3.Dot(transform.right, v),
                y = Vector3.Dot(transform.forward, v)
            };

            return Mathf.Atan2(dragPoint.y, dragPoint.x) * Mathf.Rad2Deg;
        }

        private float GetPositionFromRay(Ray ray)
        {
            var point = GetPointFromRay(ray);
            var v = point - transform.position;
            return Vector3.Dot(transform.forward, v);
        }
        
        public override void OnBeginDrag(Ray ray)
        {
            lastDragPosition = dragMode switch
            {
                DragMode.Dial => GetAngleFromDragPoint(ray),
                DragMode.Joystick => GetPositionFromRay(ray),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void OnContinueDrag(Ray ray, ref float value)
        {
            switch (dragMode)
            {
                case DragMode.Dial:
                    var angle = GetAngleFromDragPoint(ray);
                    var deltaAngle = Mathf.DeltaAngle(angle, lastDragPosition);
                    lastDragPosition = angle;
                    value += deltaAngle / 360.0f;
                    break;
                case DragMode.Joystick:
                    var position = GetPositionFromRay(ray);
                    var delta = position - lastDragPosition;
                    lastDragPosition = position;
                    value += delta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!handle) handle = transform.GetChild(0);
        }

        public override void DisplaySmoothedValue(float smoothedValue)
        {
            if (handle) handle.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(zeroRotation, oneRotation, smoothedValue));
        }

        private enum DragMode
        {
            Dial,
            Joystick,
        }
    }
}