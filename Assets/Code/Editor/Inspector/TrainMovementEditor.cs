using FR8Runtime.Train;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrainCarriage), true)]
    public class TrainCarriageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var targets = new TrainCarriage[this.targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                targets[i] = (TrainCarriage)this.targets[i];
            }

            base.OnInspectorGUI();

            if (targets.Length == 0) return;
            if (targets.Length == 1)
            {
                var target = targets[0];

                if (!target.Segment)
                {
                    EditorGUILayout.HelpBox("Train is Missing Spline", MessageType.Error);
                }
                else if (GUILayout.Button("Snap To Spline", GUILayout.Height(30)))
                {
                    if (!target.Segment) return;

                    var transform = target.transform;
                    var track = target.Segment;

                    var t = track.GetClosestPoint(transform.position);
                    transform.position = track.SamplePoint(t);
                    transform.rotation = Quaternion.LookRotation(track.SampleTangent(t), Vector3.up);
                }
            }
            else if (GUILayout.Button("Snap All To Spline", GUILayout.Height(30)))
            {
                foreach (var target in targets)
                {
                    if (!target.Segment) continue;

                    var transform = target.transform;
                    var track = target.Segment;

                    var t = track.GetClosestPoint(transform.position);
                    transform.position = track.SamplePoint(t);
                    transform.rotation = Quaternion.LookRotation(track.SampleTangent(t), Vector3.up);
                }
            }
        }
    }
}