using System;
using FR8Runtime.Train.Track;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrackMesh))]
    public class TrackMeshEditor : Editor
    {
        [MenuItem("Actions/Track Mesh/Refresh Track Meshes")]
        public static void FindAlLTrackMeshes()
        {
            AllAction(e => e.FindTrackMeshes());
        }

        [MenuItem("Actions/Track Mesh/Bake All Meshes")]
        public static void BakeAllTracks()
        {
            if (!EditorUtility.DisplayDialog("Bake All Meshes", "Are you sure you want to bake all Track Meshes in this Scene\nThis will take a long time", "Bake All", "Cancel")) return;
            
            AllAction(e => e.BakeMesh());
        }

        [MenuItem("Actions/Track Mesh/Clear All Meshes")]
        public static void ClearAllTracks()
        {
            if (!EditorUtility.DisplayDialog("Clear All Meshes", "Are you sure you want to delete all Track Bake Assets\nTHIS CANNOT BE UNDONE", "Delete All", "Cancel")) return;
            
            AllAction(e => e.Clear());
        }

        public static void AllAction(Action<TrackMesh> callback)
        {
            var list = FindObjectsOfType<TrackMesh>();
            foreach (var e in list)
            {
                callback(e);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var append = Application.isPlaying ? " [Cannot perform in playmode]" : string.Empty;

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button(Count("Bake{0}") + append, GUILayout.Height(30)))
                {
                    ExecuteAll(target => TrackMesh.ExecuteAndRefreshAssets(target.BakeMesh));
                }

                if (GUILayout.Button(Count("Clear Tracks{0}") + append, GUILayout.Height(30)))
                {
                    ExecuteAll(target => TrackMesh.ExecuteAndRefreshAssets(target.Clear));
                }
            }
        }

        private void ExecuteAll(Action<TrackMesh> segment)
        {
            foreach (var target in targets)
            {
                segment(target as TrackMesh);
            }
        }

        public string Count(string template) => string.Format(template, targets.Length > 1 ? $" [{targets.Length}]" : string.Empty);
    }
}