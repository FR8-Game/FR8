using FR8.Train;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrainCarriage), true)]
    public class TrainMovementEditor : Editor
    {
        public TrainCarriage Target => target as TrainCarriage;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Target.Segment)
            {
                EditorGUILayout.HelpBox("Train is Missing Spline", MessageType.Error);
            }
            else if (GUILayout.Button("Snap To Spline", GUILayout.Height(30)))
            {
                if (Target.Segment)
                {
                    var transform = Target.transform;
                    var track = Target.Segment;

                    transform.position = track.SamplePoint(track.GetClosestPoint(transform.position));
                }
            }
        }
    }
}