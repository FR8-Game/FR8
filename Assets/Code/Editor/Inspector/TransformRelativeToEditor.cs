using FR8.Runtime.Tools;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TransformRelativeTo))]
    public class TransformRelativeToEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var t = target as TransformRelativeTo;
            if (!t) return;
            
            EditorGUILayout.Space();

            
            var icon = EditorGUIUtility.IconContent(t.Locked ? "LockIcon-On" : "LockIcon").image;
            var content = new GUIContent(t.Locked ? "Locked" : "Unlocked", icon);
            if (GUILayout.Button(content, GUILayout.Height(40)))
            {
                t.Locked = !t.Locked;
            }
        }
    }
}