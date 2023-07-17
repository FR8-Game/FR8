using System;
using System.Collections;
using UnityEngine;

namespace FR8.Utility
{
    public static class UITween
    {
        public static Tween TweenIn => BuildTween(p => p);
        public static Tween TweenOut => BuildTween(p => 1.0f - p);

        public static Tween BuildTween(Func<float, float> remapping)
        {
            IEnumerator routine(CanvasGroup group, float duration, Vector2 offset)
            {
                var p0 = 0.0f;
                while (p0 < 1.0f)
                {
                    set();
                    p0 += Time.unscaledDeltaTime / duration;
                    yield return null;
                }

                p0 = 1.0f;
                set();
                
                void set()
                {
                    var p1 = Smootherstep(remapping(p0));
                    group.alpha = p1;
                    ((RectTransform)group.transform).anchoredPosition = offset * (1.0f - p1);   
                }
            }

            return routine;
        }

        private static float Smootherstep(float x) => x * x * x * (3.0f * x * (2.0f * x - 5.0f) + 10.0f);

        public delegate IEnumerator Tween(CanvasGroup group, float duration, Vector2 offset);
    }
}