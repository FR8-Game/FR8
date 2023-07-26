using FR8.Track;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrackSegment))]
    public class TrackSegmentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var trackSegment = target as TrackSegment;
            if (!trackSegment) return;

            var transform = trackSegment.transform;

            if (GUILayout.Button("Zero Origin", GUILayout.Height(30)))
            {
                Undo.RecordObject(transform, "Recenter Track");
                
                for (var i = 0; i < trackSegment.Knots.Count; i++)
                {
                    trackSegment.Knots[i] += transform.position;
                }

                transform.position = Vector3.zero;
            }
        }

        private void OnSceneGUI()
        {
            var trackSegment = target as TrackSegment;

            for (var i = 0; i < trackSegment.Knots.Count; i++)
            {
                trackSegment.Knots[i] = Handles.PositionHandle(trackSegment.Knots[i], Quaternion.identity);
            }
        }
    }
}