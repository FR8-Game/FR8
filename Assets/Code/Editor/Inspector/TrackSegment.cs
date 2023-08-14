using System;
using FR8.Train.Track;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

            if (GUILayout.Button("Zero Origin", GUILayout.Height(30)))
            {
                ShiftTransform(trackSegment, Vector3.zero, "Zero Track Origin");
            }

            if (GUILayout.Button("Center Origin", GUILayout.Height(30)))
            {
                if (trackSegment.Knots.Count > 0)
                {
                    var bounds = new Bounds(trackSegment.transform.TransformPoint(trackSegment.Knots[0]), Vector3.zero);
                    for (var i = 1; i < trackSegment.Knots.Count; i++)
                    {
                        bounds.Encapsulate(trackSegment.transform.TransformPoint(trackSegment.Knots[i]));
                    }

                    ShiftTransform(trackSegment, bounds.center, "Center Track Origin");
                }
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
    }
}