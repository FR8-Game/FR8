using UnityEngine;
using UnityEngine.UI;

namespace FR8.Interactions.Drivables
{
    [System.Serializable]
    public sealed class DriverDisplayUI
    {
        [SerializeField] private Image image;
        [SerializeField] private Vector2 inputRange = Vector2.up;
        [SerializeField] private Vector2 outputRange = Vector2.up;
        [SerializeField] private float testValue;

        public void SetValue(float newValue)
        {
            var p = Mathf.InverseLerp(inputRange.x, inputRange.y, newValue);
            var val = Mathf.Lerp(outputRange.x, outputRange.y, p);
            image.fillAmount = val;
        }
        
        public void OnValidate()
        {
            if (!image) return;
            SetValue(testValue);
        }
    }
}
