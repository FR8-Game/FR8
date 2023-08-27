using System;
using System.Collections.Generic;
using FR8Runtime.Train.Track;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;

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
            ModifyKnots((i, knot) =>
            {
                if (DoHandle(i, knot))
                {
                    foreach (var s in segments)
                    {
                        if (s == TrackSegment) continue;

                        var t = s.GetClosestPoint(knot.position);
                        var closest = s.SamplePoint(t);
                        if ((knot.position - closest).sqrMagnitude > TrackSegment.ConnectionDistance * TrackSegment.ConnectionDistance) continue;

                        knot.position = closest;

                        return true;
                    }

                    return true;
                }

                return false;
            });
        }

        private bool DoHandle(int i, Transform knot)
        {
            EditorGUI.BeginChangeCheck();
            
            var newPosition = Handles.PositionHandle(knot.position, knot.rotation);
            var newRotation = Handles.RotationHandle(knot.rotation, knot.position);
            
            if (!EditorGUI.EndChangeCheck()) return false;

            if (Keyboard.current.leftShiftKey.isPressed)
            {
                var lines = new List<Ray>();
                var knots = TrackSegment.KnotContainer();

                if (i > 0)
                {
                    var other = knots.GetChild(i - 1);
                    lines.Add(new Ray(other.position, other.forward));
                }

                if (i < knots.childCount - 1)
                {
                    var other = knots.GetChild(i + 1);
                    lines.Add(new Ray(other.position, other.forward));
                }

                if (lines.Count > 0)
                {
                    Handles.color = new Color(1.0f, 0.6f, 0.0f, 1.0f);
                    var bestScore = float.MaxValue;

                    foreach (var line in lines)
                    {
                        Handles.DrawLine(line.GetPoint(-1000.0f), line.GetPoint(1000.0f));

                        var v = newPosition - line.origin;
                        var d = Vector3.Dot(line.direction, v);
                        var p = line.origin + line.direction * d;

                        var score = (p - newPosition).magnitude;
                        if (score < bestScore)
                        {
                            newPosition = p;
                            newRotation = Quaternion.LookRotation(line.direction, Vector3.up);
                            bestScore = score;
                        }
                    }
                }
            }

            knot.position = newPosition;
            knot.rotation = newRotation;
            return true;
        }

        private void ModifyKnots(ModifyKnotCallback callback)
        {
            var knots = TrackSegment.KnotContainer();
            var knotCount = knots.childCount;
            var dirty = false;

            for (var i = 0; i < knotCount; i++)
            {
                var knot = knots.GetChild(i);

                Undo.RecordObject(knot, "Moved Track Knot");
                if (!callback(i, knot)) continue;
                dirty = true;
            }

            if (dirty) TrackSegment.OnKnotsChanged();
        }

        private delegate bool ModifyKnotCallback(int i, Transform knot);
    }
}