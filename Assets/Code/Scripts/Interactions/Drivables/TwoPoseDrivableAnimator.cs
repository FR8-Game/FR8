using FR8.Runtime.CodeUtility;
using UnityEngine;

namespace FR8.Runtime.Interactions.Drivables
{
    [System.Serializable]
    public class TwoPoseDrivableAnimator
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
        
        public void Update()
        {
            var t = animationSpring.currentPosition;
            SetPosition(t);
        }

        public void FixedUpdate()
        {
            animationSpring.Iterate(Time.deltaTime);
        }
        
        public void SetValue(float newValue)
        {
            animationSpring.Target(Mathf.InverseLerp(valueRange.x, valueRange.y, newValue));
        }

        private void SetPosition(float position)
        {
            if (handle && animatePosition) handle.localPosition = Vector3.LerpUnclamped(slidePoseAPosition, slidePoseBPosition, position);
            if (handle && animateRotation) handle.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(slidePoseARotation, slidePoseBRotation, position));
        }

        public void OnValidate(float testValue)
        {
            if (!Application.isPlaying)
            {
                SetPosition(Mathf.InverseLerp(valueRange.x, valueRange.y, testValue));
            }
        }
    }
}