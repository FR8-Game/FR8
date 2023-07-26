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
                
                var positions = new Vector3[trackSegment.Knots.Count];
                var center = Vector3.zero;
                for (var i = 0; i < trackSegment.Knots.Count; i++)
                {
                    var child = trackSegment.Knots[i];
                    Undo.RecordObject(child, "Recenter Track");
                    positions[i] = child.position;
                    center += positions[i] / trackSegment.Knots.Count;
                }

                transform.position = Vector3.zero;
                
                for (var i = 0; i < trackSegment.Knots.Count; i++)
                { 
                    trackSegment.Knots[i].position = positions[i];
                }
            }
            
            if (GUILayout.Button("Add Point", GUILayout.Height(30)))
            {
                if (trackSegment.Knots.Count == 0)
                {
                    var point = new GameObject($"Handle.{trackSegment.Knots.Count}").transform;
                    point.SetParent(transform);
                    point.SetAsLastSibling();
                    point.name = $"Knot.{point.GetSiblingIndex()}";
                    point.localPosition = Vector3.zero;
                }
                else
                {
                    var point = Instantiate(trackSegment.Knots[^1], transform);
                    point.name = $"Knot.{point.GetSiblingIndex()}";
                }

                trackSegment.OnValidate();
            }
        }

        private void OnSceneGUI()
        {
            var trackSegment = (target as TrackSegment);

            for (var i = 0; i < trackSegment.Knots.Count; i++)
            {
                var child = trackSegment.Knots[i];
                Undo.RecordObject(child, "Moved Track Segment Handle");

                child.position = Handles.PositionHandle(child.position, Quaternion.identity);
            }
        }
    }
}