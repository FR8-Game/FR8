using System;
using FR8.Drivers;
using UnityEngine;

namespace FR8.Drivables
{
    public class TwoPoseDrivableAnimator : MonoBehaviour, IDrivable
    {
        [SerializeField] private Transform handle;
        [SerializeField] private Vector2 valueRange = Vector2.up;
        [SerializeField] private float smoothTime = 0.06f;
        [SerializeField] private bool animatePosition;
        [SerializeField] private Vector3 slidePoseAPosition;
        [SerializeField] private Vector3 slidePoseBPosition;
        [SerializeField] private bool animateRotation;
        [SerializeField] private Vector3 slidePoseARotation;
        [SerializeField] private Vector3 slidePoseBRotation;

        private float targetValue;
        private float valueVelocity;
        private float smoothedValue;

        private void Update()
        {
            smoothedValue = Mathf.SmoothDamp(smoothedValue, targetValue, ref valueVelocity, smoothTime);
            
            if (handle && animatePosition) handle.localPosition = Vector3.LerpUnclamped(slidePoseAPosition, slidePoseBPosition, smoothedValue);
            if (handle && animateRotation) handle.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(slidePoseARotation, slidePoseBRotation, smoothedValue));
        }

        public void SetValue(DriverGroup group, float value)
        {
            targetValue = Mathf.InverseLerp(valueRange.x, valueRange.y, value);
        }

        private void OnValidate()
        {
            if (!handle) handle = transform.GetChild(0);
        }
    }
}