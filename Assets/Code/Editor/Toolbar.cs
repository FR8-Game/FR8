using System.IO;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace FR8Editor
{
    [InitializeOnLoad]
    public static class Toolbar
    {
        private static int scene;
        
        static Toolbar()
        {
            ToolbarExtender.RightToolbarGUI.Add(SceneSwitchGUI);
            ToolbarExtender.RightToolbarGUI.Add(PlayFromStartGUI);
        }

        private static void SceneSwitchGUI()
        {
            var buildScenes = EditorBuildSettings.scenes;
            var currentScene = SceneManager.GetActiveScene();

            if (EditorGUILayout.DropdownButton(new GUIContent("Switch Scenes"), FocusType.Passive, GUILayout.Width(120)))
            {
                var menu = new GenericMenu();
                foreach (var e in buildScenes)
                {
                    if (currentScene.path == e.path) continue;
                    
                    var name = Path.GetFileNameWithoutExtension(e.path);
                    
                    menu.AddItem(new GUIContent(name), false, () =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(e.path);
                        }
                    });
                }
                menu.ShowAsContext();
            }
        }

        private static void PlayFromStartGUI()
        {
            if (GUILayout.Button("Play From Start", GUILayout.Width(100)))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
                EditorApplication.isPlaying = true;
            }
        }
    }
}
