using System;
using FR8Runtime.Interactions.Drivers;
using TMPro;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivables
{
    [Serializable]
    public class DriverDisplayText
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private string label;
        [SerializeField] private int round = 0;
        [SerializeField] private float monospace;
        [SerializeField] private float valueSmoothing;

        private float targetValue;
        private float currentValue;
        
        private Func<RadialDisplay.DisplayMode> displayMode;

        public void Awake(Func<RadialDisplay.DisplayMode> displayMode)
        {
            this.displayMode = displayMode;
        }
        
        public void SetValue(float newValue)
        {
            targetValue = newValue;
        }

        public void ApplyValue()
        {
            if (!text) return;
            text.text = $"{Format(currentValue)}\n<size=30%>{label}</size>";
        }

        public void FixedUpdate()
        {
            if (valueSmoothing < Time.deltaTime) currentValue = targetValue;
            else currentValue += (targetValue - currentValue) / valueSmoothing * Time.deltaTime;

            ApplyValue();
        }
        
        public string Format(float value)
        {
            var displayMode = this.displayMode();

            switch (displayMode)
            {
                case RadialDisplay.DisplayMode.Percentage:
                {
                    value = Mathf.Clamp01(value) * 100.0f;
                    break;
                }
                case RadialDisplay.DisplayMode.Raw:
                default:
                    break;
            }
            
            if (round > -1)
            {
                var sf = Mathf.Pow(10.0f, round);
                value = Mathf.Round(value * sf) / sf;
            }
            
            var res = displayMode switch
            {
                RadialDisplay.DisplayMode.Percentage => $"{value}%",
                _ => value.ToString(),
            };

            if (monospace > 0.0f)
            {
                res = $"<mspace={monospace}em>{res}</mspace>";
            }
            
            return res;
        }

        public void SetText(string text)
        {
            if (!this.text) return;
            this.text.text = text;
        }

        public void OnValidate(float testValue)
        {
            currentValue = testValue;
            ApplyValue();
        }
    }
}
