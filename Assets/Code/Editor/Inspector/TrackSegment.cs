using System;
using System.Drawing.Drawing2D;
using FR8.Splines;
using FR8.Track;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;
using UnityEngine.XR;

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

            if (GUILayout.Button("Recenter", GUILayout.Height(30)))
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

                transform.position = center;
                
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
                    point.localPosition = Vector3.zero;
                }
                else
                {
                    Instantiate(trackSegment.Knots[transform.childCount - 1], transform);
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