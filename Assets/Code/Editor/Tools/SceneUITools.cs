using UnityEditor;
using UnityEngine;

namespace FR8Editor.Tools
{
    [InitializeOnLoad]
    public static class SceneUITools
    {
        [MenuItem("Actions/Hide UI/Show")]
        public static void ShowUI() => ShowUI(true);
        
        [MenuItem("Actions/Hide UI/Hide")]
        public static void HideUI() => ShowUI(false);

        private static string ShownUIKey => $"{nameof(SceneUITools)}.ShowUI";
        
        static SceneUITools()
        {
            UnityToolbarExtender.ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            if (!EditorPrefs.HasKey(ShownUIKey)) GetApproximateStateFromScene();
            ShowUI(EditorPrefs.GetBool(ShownUIKey));
        }

        private static void OnUndoRedoPerformed()
        {
            GetApproximateStateFromScene();
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            
            var isShown = EditorPrefs.GetBool(ShownUIKey, true);
            
            var icon = EditorGUIUtility.IconContent(isShown ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff").image;
            var content = new GUIContent($"{(isShown ? "Hide" : "Show")} UI", icon);
            if (GUILayout.Button(content, GUILayout.Width(80)))
            {
                ShowUI(!isShown);
            }
        }

        private static void ShowUI(bool show)
        {
            EditorPrefs.SetBool(ShownUIKey, show);
            
            var canvasList = Object.FindObjectsOfType<Canvas>(true);
            
            Undo.RecordObject(SceneVisibilityManager.instance, $"Set UI Visibility [{show}]");

            foreach (var e in canvasList)
            {
                if (show) SceneVisibilityManager.instance.Show(e.gameObject, true);
                else SceneVisibilityManager.instance.Hide(e.gameObject, true);
            }
        }

        private static void GetApproximateStateFromScene()
        {
            var shownCount = 0;
            var hiddenCount = 0;
            var canvasList = Object.FindObjectsOfType<Canvas>(true);
            
            foreach (var e in canvasList)
            {
                if (SceneVisibilityManager.instance.IsHidden(e.gameObject)) hiddenCount++;
                else shownCount++;
            }
            EditorPrefs.SetBool(ShownUIKey, shownCount >= hiddenCount);
        }
    }
}
