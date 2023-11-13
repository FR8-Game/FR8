using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FR8Editor.Extras
{
    public static class SceneActions
    {
        [MenuItem("Actions/Scene/Center Scene Around Selection")]
        public static void CenterSceneAroundSelection() => CenterSceneAroundSelection(new Vector3(-1.0f, 0.0f, -1.0f));
        
        
        [MenuItem("Actions/Scene/Set Scene Elevation Around Selection")]
        public static void SetSceneElevationAroundSelection() => CenterSceneAroundSelection(new Vector3(0.0f, -1.0f, 0.0f));

        private static void CenterSceneAroundSelection(Vector3 scalar)
        {
            var center = Vector3.zero;
            var list = Selection.gameObjects;

            if (list.Length == 0)
            {
                Debug.LogError("Cannot center scene, No GameObjects are selected");
                return;
            }
            
            foreach (var locomotive in list)
            {
                center += locomotive.transform.position / list.Length;
            }

            var offset = new Vector3
            {
                x = scalar.x * center.x,
                y = scalar.y * center.y,
                z = scalar.z * center.z,
            };

            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            foreach (var e in rootObjects)
            {
                Undo.RecordObject(e.transform, "Center Scene Around Locomotive");
                e.transform.position += offset;
            }
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}