using FR8Runtime.Train;
using FR8Runtime.Train.Track;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrainCarriage), true)]
    public class TrainCarriageEditor : Editor
    {
        private void OnSceneGUI()
        {
            var train = target as TrainCarriage;
            if (!train.Segment) return;

            var a = train.transform.position;
            var p = Application.isPlaying ? train.PositionOnSpline : train.Segment.GetClosestPoint(a);

            var b = train.Segment.SamplePoint(p);
            var t = train.Segment.SampleTangent(p);

            Handles.color = new Color(0.04f, 1f, 0.45f);
            Handles.DrawAAPolyLine(a, b);
            Handles.DrawWireArc(b, t, Vector3.up, 360.0f, 2.0f);
        }

        public override void OnInspectorGUI()
        {
            var targets = new TrainCarriage[this.targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                targets[i] = (TrainCarriage)this.targets[i];
            }

            base.OnInspectorGUI();

            if (PrefabStageUtility.GetCurrentPrefabStage()) return;

            if (targets.Length == 0) return;
            if (targets.Length == 1)
            {
                var target = targets[0];

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(target.Segment, typeof(TrackSegment), false);
                }
                
                if (GUILayout.Button("Snap To Spline", GUILayout.Height(30)))
                {
                    SnapToSpline(target);
                }
            }
            else if (GUILayout.Button("Snap All To Spline", GUILayout.Height(30)))
            {
                foreach (var target in targets)
                {
                    SnapToSpline(target);
                }
            }
        }

        private void SnapToSpline(TrainCarriage target)
        {
            Undo.RecordObject(target, "Snapped to Splines");
            Undo.RecordObject(target.transform, "Snapped to Splines");

            target.FindClosestSegment();

            var transform = target.transform;
            var track = target.Segment;

            var t = track.GetClosestPoint(transform.position);
            transform.position = track.SamplePoint(t);
            transform.rotation = Quaternion.LookRotation(track.SampleTangent(t), Vector3.up);
        }
    }
}