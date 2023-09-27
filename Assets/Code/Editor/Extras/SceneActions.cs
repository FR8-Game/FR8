
using FR8Runtime.Train;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FR8Editor.Extras
{
    public static class SceneActions
    {
        [MenuItem("Actions/Scene/Center Scene Around Locomotive")]
        public static void CenterSceneAroundLocomotive()
        {
            if (!EditorUtility.DisplayDialog("Center Scene Around Locomotive", "Are you sure?\nTHIS ACTION CANNOT BE UNDONE", "Yes", "Cancel")) return;
            if (!EditorUtility.DisplayDialog("Center Scene Around Locomotive", "Are you really sure???\nTHIS ACTION CANNOT BE UNDONE", "Absolutely", "Cancel")) return;

            var offset = Vector3.zero;
            var list = Object.FindObjectsOfType<Locomotive>();
            foreach (var locomotive in list)
            {
                offset += locomotive.transform.position / list.Length;
            }

            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var e in rootObjects)
            {
                e.transform.position -= offset;
            }
        }
    }
}