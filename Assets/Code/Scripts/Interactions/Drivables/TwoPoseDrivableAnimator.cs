using FR8.Interactions.Drivers;
using FR8.Utility;
using UnityEngine;

namespace FR8.Interactions.Drivables
{
    public class TwoPoseDrivableAnimator : MonoBehaviour, IDrivable
    {
        [Header("HANDLE SETTINGS")]
        [Space]
        [SerializeField] private Transform handle;
        [SerializeField] private Vector2 valueRange = Vector2.up;

        [Header("HANDLE SETTINGS")]
        [Space]
        [SerializeField] private DampedSpring animationSpring;
        
        [Header("POSE PARAMETERS")]
        [Space]
        [SerializeField] private bool animatePosition;
        [SerializeField] private Vector3 slidePoseAPosition;
        [SerializeField] private Vector3 slidePoseBPosition;
        [SerializeField] private bool animateRotation;
        [SerializeField] private Vector3 slidePoseARotation;
        [SerializeField] private Vector3 slidePoseBRotation;

        private void Update()
        {
            var t = animationSpring.currentPosition;
            if (handle && animatePosition) handle.localPosition = Vector3.LerpUnclamped(slidePoseAPosition, slidePoseBPosition, t);
            if (handle && animateRotation) handle.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(slidePoseARotation, slidePoseBRotation, t));
        }

        private void FixedUpdate()
        {
            animationSpring.Iterate(Time.deltaTime);
        }

        public void SetValue(DriverGroup group, float value)
        {
            animationSpring.Target(Mathf.InverseLerp(valueRange.x, valueRange.y, value));
        }

        private void OnValidate()
        {
            if (!handle && transform.childCount > 0) handle = transform.GetChild(0);
        }
    }
}