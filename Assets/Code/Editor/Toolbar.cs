using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var buildScenes = GetBuildScenes();
            var currentScenePath = SceneManager.GetActiveScene().path;
            var currentSceneGuid = AssetDatabase.AssetPathToGUID(currentScenePath);
            var menu = new GenericMenu();

            if (EditorGUILayout.DropdownButton(new GUIContent(Path.GetFileNameWithoutExtension(currentScenePath)), FocusType.Passive, GUILayout.ExpandWidth(false)))
            {
                var otherScenes = GetAllScenes(buildScenes);
             
                addScene(currentScenePath, "Reload ");
                menu.AddSeparator(string.Empty);
                listScenes(buildScenes);
                menu.AddSeparator(string.Empty);
                listScenes(otherScenes);

                menu.ShowAsContext();
            }

            void listScenes(IEnumerable<string> list)
            {
                foreach (var e in list)
                {
                    if (currentSceneGuid == e) continue;
                    
                    var path = AssetDatabase.GUIDToAssetPath(e);

                    if (path.Contains("Packages/")) continue;
                    if (path.Contains("Plugins/")) continue;

                    addScene(path);
                }
            }

            void addScene(string path, string prepend = "", string append = "")
            {
                var name = Path.GetFileNameWithoutExtension(path);
                menu.AddItem(new GUIContent(prepend + name + append), false, () =>
                {
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                    EditorSceneManager.OpenScene(path);
                });
            }
        }

        private static List<string> GetBuildScenes()
        {
            var res = new List<string>();
            
            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                res.Add(editorScene.guid.ToString());
            }

            return res;
        }
        
        private static List<string> GetAllScenes(List<string> excludeList)
        {
            var guids = AssetDatabase.FindAssets("t:scene");
            var res = new List<string>();

            foreach (var guid in guids)
            {
                var ignore = false;
                foreach (var e in excludeList)
                {
                    if (guid != e) continue;
                    ignore = true;
                    break;
                }

                if (ignore) continue;
                
                res.Add(guid);
            }

            return res;
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
