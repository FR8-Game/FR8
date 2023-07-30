using FR8.Train.Track;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrackModel))]
    public class TrackModelEditor : Editor
    {
        private bool autoBakeMesh = false;

        [MenuItem("Actions/Track Model/Bake All Meshes")]
        public static void BakeAllTracks()
        {
            var list = FindObjectsOfType<TrackModel>();
            foreach (var e in list)
            {
                e.BakeMesh();
            }
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var target = this.target as TrackModel;
            if (!target) return;

            // ReSharper disable once AssignmentInConditionalExpression
            if (autoBakeMesh = EditorGUILayout.Toggle("Auto Bake Mesh", autoBakeMesh))
            {
                target.BakeMesh();
            }
            
            if (GUILayout.Button("Bake", GUILayout.Height(30)))
            {
                target.BakeMesh();
            }
        }
    }
}