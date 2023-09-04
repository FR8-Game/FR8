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
        [MenuItem("Actions/Track Model/Bake All Meshes")]
        public static void BakeAllTracks()
        {
            var list = FindObjectsOfType<TrackMesh>();
            foreach (var e in list)
            {
                e.BakeMesh();
            }
        }

        [MenuItem("Actions/Track Model/Clear All Meshes")]
        public static void ClearAllTracks()
        {
            var list = FindObjectsOfType<TrackMesh>();
            foreach (var e in list)
            {
                e.Clear();
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