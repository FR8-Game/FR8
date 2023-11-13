
using System;
using UnityEngine;

namespace FR8.Runtime.Player
{
    public class Toast
    {
        private const float HoldTime = 5.0f;
        private const float FadeTime = 2.0f;
            
        public string text;
        public Color color;
        public float startTime;

        public static event Action<Toast> ShowToastEvent;
        
        public static void ShowToast(string text, Color color)
        {
            ShowToastEvent?.Invoke(new Toast(text, color));
        }

        public Toast(string text, Color color)
        {
            this.text = text;
            this.color = color;
            startTime = Time.time;
        }

        public string Build()
        {
            var age = Time.time - startTime;
            var alpha = 1.0f - Mathf.Max(0.0f, age - HoldTime) / FadeTime;
            var color = this.color;
            color.a *= alpha;
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text.ToUpper()}";
        }

        public bool Expired() => (Time.time - startTime) > (HoldTime + FadeTime);
    }
}