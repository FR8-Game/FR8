using System;
using FR8Runtime.Interactions.Drivers;
using UnityEngine;
using UnityEngine.UI;

namespace FR8Runtime.Interactions.Drivables
{
    [System.Serializable]
    public sealed class DriverDisplayUI
    {
        [SerializeField] private Image image;
        [SerializeField] private Vector2 inputRange = Vector2.up;
        [SerializeField] private Vector2 outputRange = Vector2.up;

        private Func<RadialDisplay.DisplayMode> displayMode;

        public void Awake(Func<RadialDisplay.DisplayMode> displayMode)
        {
            this.displayMode = displayMode;
        }

        public void SetValue(float newValue)
        {
            var p = displayMode() switch
            {
                RadialDisplay.DisplayMode.Percentage => newValue, 
                _ => Mathf.InverseLerp(inputRange.x, inputRange.y, newValue),
            };
            SetValueNormalized(p);
        }

        public void SetValueNormalized(float percent)
        {
            if (!image) return;
            
            var val = Mathf.Lerp(outputRange.x, outputRange.y, percent);
            image.fillAmount = val;
        }

        public void OnValidate(float testValue)
        {
            if (!image) return;
            SetValue(testValue);
        }
    }
}