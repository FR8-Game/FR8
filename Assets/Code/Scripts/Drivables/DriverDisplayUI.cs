using System;
using FR8.Drivers;
using UnityEngine;
using UnityEngine.UI;

namespace FR8.Drivables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class DriverDisplayUI : MonoBehaviour, IDrivable
    {
        [SerializeField] private Vector2 inputRange = Vector2.right;
        [SerializeField] private Vector2 outputRange = Vector2.right;
        [SerializeField] private float testValue;
        
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }
        
        public void SetValue(DriverGroup group, float value)
        {
            var p = Mathf.InverseLerp(inputRange.x, inputRange.y, value);
            var val = Mathf.Lerp(outputRange.x, outputRange.y, p);
            image.fillAmount = val;
        }
        
        private void OnValidate()
        {
            image = GetComponent<Image>();
            SetValue(null, testValue);
        }
    }
}
