using System;
using FR8Runtime.Train.Track;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrackSegment))]
    public class TrackSegmentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button(Count("Zero Track Origin{0}"), GUILayout.Height(30)))
            {
                ExecuteAll(s => ShiftTransform(s, Vector3.zero, "Zero Track Origin"));
            }

            if (GUILayout.Button(Count("Center Track Origin{0}"), GUILayout.Height(30)))
            {
                ExecuteAll(s =>
                {
                    if (s.Knots.Count <= 0) return;

                    var bounds = new Bounds(s.transform.TransformPoint(s.Knots[0]), Vector3.zero);
                    for (var i = 1; i < s.Knots.Count; i++)
                    {
                        bounds.Encapsulate(s.transform.TransformPoint(s.Knots[i]));
                    }

                    ShiftTransform(s, bounds.center, "Center Track Origin");
                });
            }
        }

        private void ShiftTransform(TrackSegment segment, Vector3 newPosition, string undoText)
        {
            Undo.RecordObjects(new Object[] { segment.transform, segment }, undoText);

            var worldPoints = new Vector3[segment.Knots.Count];

            for (var i = 0; i < worldPoints.Length; i++)
            {
                worldPoints[i] = segment.transform.TransformPoint(segment.Knots[i]);
            }

            segment.transform.position = newPosition;

            for (var i = 0; i < worldPoints.Length; i++)
            {
                segment.Knots[i] = segment.transform.InverseTransformPoint(worldPoints[i]);
            }
        }

        private void OnSceneGUI()
        {
            var trackSegment = target as TrackSegment;

            Undo.RecordObject(trackSegment, "Moved Track Segment");
            for (var i = 0; i < trackSegment.Knots.Count; i++)
            {
                var worldPos = trackSegment.transform.TransformPoint(trackSegment.Knots[i]);
                worldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

                trackSegment.Knots[i] = trackSegment.transform.InverseTransformPoint(worldPos);
            }
        }

        private void ExecuteAll(Action<TrackSegment> segment)
        {
            foreach (var target in targets)
            {
                segment(target as TrackSegment);
            }
        }

        public string Count(string template) => string.Format(template, targets.Length > 1 ? $" [{targets.Length}]" : string.Empty);
    }
}