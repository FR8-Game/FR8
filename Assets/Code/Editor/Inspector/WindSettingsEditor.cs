using FR8.Runtime.Level;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WindSettings))]
    public class WindSettingsEditor : Editor
    {
        private float t;

        public override void OnInspectorGUI()
        {
            var color = new Color(1.0f, 0.6f, 0.2f, 1.0f);
            
            base.OnInspectorGUI();

            var superRect = EditorGUILayout.GetControlRect(false, 100.0f);
            var rect = superRect;
            rect.xMax -= 20.0f;
            
            EditorGUI.DrawRect(rect, Color.black);

            var wind = target as WindSettings;

            Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
            Handles.DrawLine(mapToRect(0.0f, 1.0f), mapToRect(1.0f, 1.0f));

            var step = 1.0f / 10;
            for (var p0 = 0.0f; p0 < 1.0f; p0 += step)
            {
                var p1 = ((p0 - t) % 1.0f + 1.0f) % 1.0f;
                Handles.DrawLine(mapToRect(p1, 0.0f), mapToRect(p1, 2.5f));
            }

            Handles.color = color;
            step = 1.0f / Mathf.Clamp(rect.width, 10.0f, 500.0f);
            for (var p = 0.0f; p < 1.0f; p += step)
            {
                var t0 = p;
                var t1 = p + step;

                var s0 = wind.GetNoise(t + t0);
                var s1 = wind.GetNoise(t + t1);

                Handles.DrawAAPolyLine(3.0f, mapToRect(t0, s0), mapToRect(t1, s1));
            }

            rect = superRect;
            rect.xMin = rect.xMax - 10.0f;
            EditorGUI.DrawRect(rect, Color.black);
            rect.xMax -= 2.0f;
            rect.xMin += 2.0f;
            rect.yMin += 2.0f;
            rect.yMax -= 2.0f;

            rect.yMin += rect.height * (1.0f - wind.GetNoise(t + 1.0f) / 2.0f);
            EditorGUI.DrawRect(rect, color);

            t += Time.deltaTime;
            Repaint();

            Vector2 mapToRect(float x, float y)
            {
                return new Vector2
                {
                    x = Mathf.Lerp(rect.xMin, rect.xMax, x),
                    y = Mathf.Lerp(rect.yMax, rect.yMin, y / 2.5f),
                };
            }
        }
    }
}