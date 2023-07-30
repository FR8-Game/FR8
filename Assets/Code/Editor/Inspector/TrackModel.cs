using FR8.Train.Track;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrackModel))]
    public class TrackModelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var target = base.target as TrackModel;
            if (!target) return;

            if (GUILayout.Button("Bake", GUILayout.Height(30)))
            {
                target.BakeMesh();
            }
        }
    }
}