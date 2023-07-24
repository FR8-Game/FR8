using UnityEditor;
using UnityEngine;

namespace FR8Editor.BuildMage
{
    [CustomEditor(typeof(BuildMage))]
    public class BuildMageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var buildMage = target as BuildMage;
            if (!buildMage) return;
            
            if (GUILayout.Button($"Build version v{Application.version}", GUILayout.Height(40)))
            {
                buildMage.Build();
            }
        }
    }
}