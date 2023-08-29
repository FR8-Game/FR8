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
                    var knots = s.KnotContainer();
                    var knotCount = knots.childCount;
                    
                    if (knotCount <= 0) return;

                    var bounds = new Bounds(knots.GetChild(0).position, Vector3.zero);
                    for (var i = 1; i < knotCount; i++)
                    {
                        bounds.Encapsulate(knots.GetChild(i).position);
                    }

                    ShiftTransform(s, bounds.center, "Center Track Origin");
                });
            }
        }

        private void ShiftTransform(TrackSegment segment, Vector3 newPosition, string undoText)
        {
            Undo.RecordObjects(new Object[] { segment.transform, segment }, undoText);

            var knots = segment.KnotContainer();
            var knotCount = knots.childCount;
            
            var worldPoints = new Vector3[knotCount];

            for (var i = 0; i < worldPoints.Length; i++)
            {
                worldPoints[i] = knots.GetChild(i).position;
            }

            segment.transform.position = newPosition;

            for (var i = 0; i < worldPoints.Length; i++)
            {
                knots.GetChild(i).position = worldPoints[i];
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