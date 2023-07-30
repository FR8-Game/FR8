using System.IO;
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

            var disable = !buildMage.rebuild && !buildMage.pushToItch && !buildMage.notifyDiscord;
            
            if (!IsButlerInstalled())
            {
                EditorGUILayout.HelpBox("Butler has not been installed, please install it at \"C:\\Butler\\\"", MessageType.Error);
                return;
            }
            
            if (disable) EditorGUI.BeginDisabledGroup(true);

            if (GUILayout.Button($"Build version v{Application.version}", GUILayout.Height(40)))
            {
                buildMage.Build();
            }
        }

        public static bool IsButlerInstalled()
        {
            return File.Exists("C:/butler/butler.exe");
        }
    }
}