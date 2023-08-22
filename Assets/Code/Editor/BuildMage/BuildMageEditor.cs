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
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.5f);

            var sceneList = EditorBuildSettings.scenes;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label($"Building {sceneList.Length} {(sceneList.Length == 1 ? "Scene" : "Scenes")}");
                foreach (var scene in sceneList)
                {
                    GUILayout.Label($" - {scene.path}");
                }
            }
            
            var disable = (!buildMage.rebuild && !buildMage.pushToItch && !buildMage.notifyDiscord) || sceneList.Length == 0;
            
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