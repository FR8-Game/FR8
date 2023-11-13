using FR8.Runtime.Train.Electrics;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrainElectricsController))]
    public class TrainElectricsControllerEditor : Editor
    {
        private bool infoFoldout;

        private void OnEnable()
        {
            infoFoldout = EditorPrefs.GetBool($"{GetType().FullName}infoFoldout", infoFoldout);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool($"{GetType().FullName}infoFoldout", infoFoldout);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var electricsController = target as TrainElectricsController;
            if (!electricsController) return;

            // ReSharper disable once AssignmentInConditionalExpression
            if (infoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(infoFoldout, "Info"))
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.FloatField("Power Draw [MW]", Mathf.Round(electricsController.PowerDraw));
                        EditorGUILayout.FloatField("Power Capacity [MW]", Mathf.Round(electricsController.Capacity));
                        EditorGUILayout.FloatField("Power Saturation [%]", Mathf.Round(electricsController.Saturation * 100.0f));
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}