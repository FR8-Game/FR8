using System;
using FR8Runtime.Train.Track;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace FR8Editor.Tools
{
    [EditorTool("Track Spline Tool", typeof(TrackSegment))]
    public class TrackSegmentTool : EditorTool
    {
        private Transform Transform => TrackSegment.transform;
        private TrackSegment TrackSegment => target as TrackSegment;

        public override void OnToolGUI(EditorWindow window)
        {
            var segments = FindObjectsOfType<TrackSegment>();

            Undo.RecordObject(TrackSegment, "Moved Track Segment");
            ModifyKnots(knot =>
            {
                EditorGUI.BeginChangeCheck();

                var newPos = DrawHandle(knot);

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var s in segments)
                    {
                        if (s == TrackSegment) continue;

                        var t = s.GetClosestPoint(newPos);
                        var closest = s.SamplePoint(t);
                        if ((newPos - closest).sqrMagnitude > TrackSegment.ConnectionDistance * TrackSegment.ConnectionDistance) continue;
                        
                        knot.position = closest;
                        return true;
                    }

                    knot.position = newPos;
                    return true;
                }
                return false;
            });
        }

        private Vector3 DrawHandle(Transform knot)
        {
            return Handles.PositionHandle(knot.position, Quaternion.identity);
        }

        private void ModifyKnots(ModifyKnotCallback callback)
        {
            var knots = TrackSegment.KnotContainer();
            var knotCount = knots.childCount;
            var dirty = false;

            for (var i = 0; i < knotCount; i++)
            {
                var knot = knots.GetChild(i);

                if (!callback(knot)) continue;
                dirty = true;
            }

            if (dirty) TrackSegment.OnKnotsChanged();
        }

        private delegate bool ModifyKnotCallback(Transform knot);
    }
}