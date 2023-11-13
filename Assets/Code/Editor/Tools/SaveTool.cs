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

        private bool autoReload;
        private List<string> log = new();

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Save Data");
                EditorGUI.indentLevel++;
                
                var save = SaveManager.ProgressionSave.data;
                if (autoReload)
                {
                    save = SaveManager.ProgressionSave.GetOrLoad();
                    Repaint();
                }
                
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
                
                EditorGUI.indentLevel--;
            }

            autoReload = EditorGUILayout.Toggle("Auto Reload", autoReload);

            using (new EditorGUI.DisabledScope(autoReload))
            {
                if (GUILayout.Button("Load")) SaveManager.ProgressionSave.Load();
                if (GUILayout.Button("Save")) SaveManager.ProgressionSave.Save();
            }

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
            int i => EditorGUILayout.IntField(FormatFieldName(field.Name), i),
            _ => value
        };

        private string FormatFieldName(string name)
        {
            var res = string.Empty;

            var c = name[0];
            res += c.ToString().ToUpper();
            for (var i = 1; i < name.Length; i++)
            {
                c = name[i];
                if (c >= 'A' && c <= 'Z')
                {
                    res += $" {c}";
                }
                
                res += c;
            }
            
            return res;
        }
    }
}