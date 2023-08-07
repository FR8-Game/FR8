using System;
using FR8.Interactions.Drivers;
using TMPro;
using UnityEngine;

namespace FR8.Interactions.Drivables
{
    [Serializable]
    public class DriverDisplayText
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private string label;
        [SerializeField] private int round = 0;
        [SerializeField] private float monospace;

        private Func<RadialDisplay.DisplayMode> displayMode;

        public void Awake(Func<RadialDisplay.DisplayMode> displayMode)
        {
            this.displayMode = displayMode;
        }
        
        public void SetValue(float newValue)
        {
            if (!text) return;
            text.text = $"{Format(newValue)}\n<size=30%>{label}</size>";
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
            SetValue(testValue);
        }
    }
}
