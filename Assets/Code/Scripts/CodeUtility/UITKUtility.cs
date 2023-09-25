
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.CodeUtility
{
    public static class UitkUtility
    {
        public static IEnumerator Typewriter(float cps, float elementDelay, params TextElement[] list)
        {
            var text = new string[list.Length];
            for (var i = 0; i < list.Length; i++)
            {
                text[i] = list[i].text;
                list[i].text = "";
            }

            return routine();
            
            IEnumerator routine()
            {
                for (var i = 0; i < list.Length; i++)
                {
                    yield return TypewriterRoutine(text[i], list[i], cps, elementDelay);
                }
            }
        }

        private static IEnumerator TypewriterRoutine(string str, TextElement target, float cps, float postDelay)
        {
            target.text = "";
            target.visible = true;

            var duration = str.Length / cps;
            var p = 0.0f;
            while (p < 1.0f)
            {
                var characterCount = Mathf.FloorToInt((str.Length - 1) * p) + 1;

                target.text = $"{str[..characterCount]}_<alpha=#00>{str[(characterCount + 1)..]}</alpha>";

                p += Time.unscaledDeltaTime / duration;
                yield return null;
            }

            target.text = $"{str}_";
            yield return new WaitForSecondsRealtime(postDelay);
            target.text = str;
        }
    }
}