using System;
using System.Collections.Generic;
using System.Reflection;
using FR8.Runtime.Save;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Tools
{
    public class SaveTool : EditorWindow
    {
        [MenuItem("Tools/Save Editor")]
        public static void Open() => CreateWindow<SaveTool>();

        private List<string> log;

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var save = SaveManager.ProgressionSave.data;
                if (save != null)
                {
                    var fields = save.GetType().GetFields();
                    foreach (var field in fields)
                    {
                        var value = field.GetValue(save);
                        value = FieldField(field, value);
                        field.SetValue(save, value);
                    }
                }
                else EditorGUILayout.LabelField("<null>");
            }

            if (GUILayout.Button("Load")) SaveManager.ProgressionSave.Load();
            if (GUILayout.Button("Save")) SaveManager.ProgressionSave.Save();

            var logBuild = string.Empty;
            for (var i = log.Count; i > 0; i++)
            {
                var l = log[i - 1];
                logBuild += l;
            }

            EditorGUILayout.TextArea(logBuild, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 4));

            if (GUILayout.Button("Clear Log"))
            {
                log.Clear();
            }
        }

        private object FieldField(FieldInfo field, object value) => value switch
        {
            int i => EditorGUILayout.IntField(field.Name, i),
            _ => value
        };
    }
}