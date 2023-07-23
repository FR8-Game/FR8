using System;
using FR8.Train;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrainMovement))]
    public class TrainMovementEditor : Editor
    {
        private bool infoFoldout = true;
        private float maxTestSpeed = 100.0f;

        public TrainMovement Target => target as TrainMovement;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // ReSharper disable once AssignmentInConditionalExpression
            if (!(infoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(infoFoldout, "Info"))) return;

            GetBrakeParameters(out var brakeMax);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.FloatField("Max Speed", 0.0f);
                }

                GUILayout.Label("Brake Curve");


                var graph = StartGraph(160.0f);
                DrawGraph(graph, Target.CalculateBrakeForce, 0.0f, maxTestSpeed / 3.6f, brakeMax * -0.1f, brakeMax * 1.1f, Color.white);

                if (Application.isPlaying)
                {
                    var fwdSpeed = Mathf.Abs(Vector3.Dot(Target.transform.forward, Target.Rigidbody.velocity));
                    DrawGraph(graph, _ => fwdSpeed * Target.Brake, 0.0f, maxTestSpeed / 3.6f, brakeMax * -0.1f, brakeMax * 1.1f, new Color(1.0f, 1.0f, 0.0f, 1.0f), true);
                    DrawGraph(graph, _ => fwdSpeed, 0.0f, maxTestSpeed / 3.6f, brakeMax * -0.1f, brakeMax * 1.1f, new Color(1.0f, 1.0f, 0.0f, 0.3f), true);
                }

                maxTestSpeed = EditorGUILayout.FloatField("Max Speed [KM/H]", maxTestSpeed);
            }
        }

        private Rect StartGraph(float height)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawRect(rect, Color.black);
            return rect;
        }

        private void DrawGraph(Rect rect, Func<float, float> function, float domainMin, float domainMax, float rangeMin, float rangeMax, Color color, bool flip = false)
        {
            line(Vector2.up * rangeMin, Vector2.up * rangeMax, new Color(1.0f, 1.0f, 1.0f, 0.2f));
            line(Vector2.right * domainMin, Vector2.right * domainMax, new Color(1.0f, 1.0f, 1.0f, 0.2f));

            var step = (domainMax - domainMin) / 500.0f;
            for (var x0 = domainMin; x0 <= domainMax - step; x0 += step)
            {
                var x1 = x0 + step;

                if (flip) line(new Vector2(function(x0), x0), new Vector2(function(x1), x1), color);
                else line(new Vector2(x0, function(x0)), new Vector2(x1, function(x1)), color);
            }

            void line(Vector2 a, Vector2 b, Color color)
            {
                a = graphToDrawSpace(a);
                b = graphToDrawSpace(b);

                Handles.color = color;
                Handles.DrawAAPolyLine(a, b);
            }

            Vector2 graphToDrawSpace(Vector2 position) => new()
            {
                x = Mathf.LerpUnclamped(rect.x, rect.xMax, Mathf.InverseLerp(domainMin, domainMax, position.x)),
                y = Mathf.LerpUnclamped(rect.y, rect.yMax, 1.0f - Mathf.InverseLerp(rangeMin, rangeMax, position.y)),
            };
        }

        private void GetBrakeParameters(out float brakeMax)
        {
            brakeMax = serializedObject.FindProperty("brakeMax").floatValue;
        }
    }
}