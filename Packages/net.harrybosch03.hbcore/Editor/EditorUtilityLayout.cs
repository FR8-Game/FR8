using UnityEditor;
using UnityEngine;
using ColorUtility = HBCore.Utility.ColorUtility;

namespace HBCoreEditor
{
    public static class EditorUtilityLayout
    {
        public static void Div()
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            rect.y += rect.height / 2.0f;
            rect.height = 1.0f;
            EditorGUI.DrawRect(rect, ColorUtility.Gray(1.0f, 0.2f));
        }
    }
}