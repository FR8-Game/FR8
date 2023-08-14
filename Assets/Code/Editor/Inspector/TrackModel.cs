using FR8.Train.Track;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [InitializeOnLoad]
    [CustomEditor(typeof(TrackModel))]
    public class TrackModelEditor : Editor
    {
        static TrackModelEditor()
        {
            EditorSceneManager.sceneSaved += _ => BakeAllTracks();
        }

        [MenuItem("Actions/Track Model/Bake All Meshes")]
        public static void BakeAllTracks()
        {
            var list = FindObjectsOfType<TrackModel>();
            foreach (var e in list)
            {
                e.BakeMesh();
            }
        }

        [MenuItem("Actions/Track Model/Clear All Meshes")]
        public static void ClearAllTracks()
        {
            var list = FindObjectsOfType<TrackModel>();
            foreach (var e in list)
            {
                e.Clear();
            }
        }

        [MenuItem("Actions/Track Model/Purge Leaked Meshes")]
        public static void PurgeLeakedMeshes()
        {
            var list = FindObjectsOfType<Mesh>();
            var regex = new Regex(@"\[PROC\].*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var c = 0;
            foreach (var e in list)
            {
                if (!regex.IsMatch(e.name)) continue;
                DestroyImmediate(e);
                c++;
            }
            Debug.Log($"Deleted {c} meshes");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var target = this.target as TrackModel;
            if (!target) return;

            var append = Application.isPlaying ? " [Cannot perform in playmode]" : string.Empty;
            
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button($"Bake{append}", GUILayout.Height(30)))
                {
                    target.BakeMesh(true);
                }

                if (GUILayout.Button($"Clear{append}", GUILayout.Height(30)))
                {
                    target.Clear();
                }
            }
        }
    }
}