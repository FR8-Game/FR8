using System;
using System.Collections;
using UnityEngine;

namespace FR8.Runtime.CodeUtility
{
    public static class UITween
    {
        public static IEnumerator BounceIn(CanvasGroup group, float duration)
        {
            return BuildTween(group, duration, _ => Vector2.zero, p => (1.0f - CurvesUtility.Bounce(p)) * 30.0f, CurvesUtility.Bounce, _ => 1.0f);
        }

        public static IEnumerator BounceOut(CanvasGroup group, float duration)
        {
            return BuildTween(group, duration, _ => Vector2.zero, p => (1.0f - CurvesUtility.Bounce(1.0f - p)) * 30.0f, p => CurvesUtility.Bounce(1.0f - p), _ => 1.0f);
        }

        public static IEnumerator BuildTween(CanvasGroup group, float duration, Func<float, Vector2> position, Func<float, float> rotation, Func<float, float> scale, Func<float, float> alpha)
        {
            var rectTransform = group.transform as RectTransform;

            var percent = 0.0f;
            while (percent < 1.0f)
            {
                set();
                percent += Time.unscaledDeltaTime / duration;
                yield return null;
            }

            percent = 1.0f;
            set();

            void set()
            {
                rectTransform.anchoredPosition = position(percent);
                rectTransform.rotation = UnityEngine.Quaternion.Euler(0.0f, 0.0f, rotation(percent));
                rectTransform.localScale = Vector3.one * scale(percent);
                group.alpha = alpha(percent);
            }
        }

        public delegate IEnumerator Tween(CanvasGroup group, float duration, Vector2 offset);
    }
}