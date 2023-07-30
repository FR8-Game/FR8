using System;
using System.Collections.Generic;
using System.Linq;
using FR8.Train.Electrics;
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
                        EditorGUILayout.FloatField("Power In Storage [MWh]", Mathf.Round(electricsController.PowerStorage));
                        EditorGUILayout.FloatField("Power Draw [MW]", Mathf.Round(electricsController.PowerDraw));
                    }

                    var electrics = new List<TrainElectrics>(electricsController.GetComponentsInChildren<TrainElectrics>()).OrderBy(e => e.FuzeGroup).ToList();
                    if (electrics.Count == 0) return;

                    var fuzeGroup = electrics[0].FuzeGroup - 1;
                    foreach (var e in electrics)
                    {
                        if (e.FuzeGroup != fuzeGroup)
                        {
                            fuzeGroup = e.FuzeGroup;
                            EditorGUILayout.LabelField($"Fuze Group {fuzeGroup}");
                        }

                        if (GUILayout.Button($"[{e.GetType().Name}] {e.gameObject.name}"))
                        {
                            EditorGUIUtility.PingObject(e.gameObject);
                        }
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}