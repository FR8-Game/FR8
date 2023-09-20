using System;
using System.Collections.Generic;
using FR8Runtime.CodeUtility;
using FR8Runtime.Train.Track;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;
using ColorUtility = HBCore.Utility.ColorUtility;

namespace FR8Editor.Tools
{
    [EditorTool("Track Spline Tool")]
    public class TrackSegmentTool : EditorTool
    {
        private static readonly Color SelectedGizmoColor = new(0f, 0.84f, 1f);
        private static readonly Color OtherGizmoColor = ColorUtility.Invert(SelectedGizmoColor);

        private TrackSegment trackSegment;
        private List<TrackSegment> all;
        private List<Terrain> terrainList;
        private Transform transform;
        private int knotMask = -1;
        private bool child;

        private (TrackSegment, int) lastSelectedSegment;

        public static float handleScale = 1.0f;

        public Transform[] Knots()
        {
            if (!trackSegment) return new Transform[0];

            if (knotMask == -1)
            {
                var res = new Transform[trackSegment.Count];
                for (var i = 0; i < res.Length; i++) res[i] = trackSegment[i];
                return res;
            }

            return knotMask >= 0 && knotMask < trackSegment.Count ? new[] { trackSegment[knotMask] } : new Transform[0];
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (all == null) all = new List<TrackSegment>(FindObjectsOfType<TrackSegment>());
            if (terrainList == null) terrainList = new List<Terrain>(FindObjectsOfType<Terrain>());

            var gameObject = target as GameObject;
            if (!gameObject) return;

            trackSegment = gameObject.GetComponent<TrackSegment>();
            if (trackSegment)
            {
                child = false;
                knotMask = -1;
            }
            else
            {
                trackSegment = gameObject.GetComponentInParent<TrackSegment>();
                if (!trackSegment) return;

                child = true;
                for (var i = 0; i < trackSegment.Count; i++)
                {
                    if (gameObject.transform != trackSegment[i]) continue;
                    knotMask = i;
                    break;
                }
            }

            transform = trackSegment.transform;

            if (lastSelectedSegment.Item1 != trackSegment)
            {
                lastSelectedSegment = (null, -1);
            }

            Undo.RecordObject(trackSegment, "Moved Track Segment");
            ModifyKnots((i, knot) =>
            {
                if (DoHandle(i, knot))
                {
                    foreach (var s in all)
                    {
                        if (s == trackSegment) continue;

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

            if (Handles.ShouldRenderGizmos())
            {
                foreach (var s in all)
                {
                    s.DrawGizmos(s == trackSegment, SelectedGizmoColor, OtherGizmoColor);
                }
            }
        }

        [MenuItem("Actions/Track Segments/Flatten Selection")]
        private static void FlattenSelectedKnots()
        {
            var selection = Selection.objects;

            foreach (var s in selection)
            {
                if (s is not GameObject go) continue;
                var t = go.transform;
                Undo.RecordObject(t, "Flattened Selected Knots");

                t.rotation = Quaternion.Euler(0.0f, t.eulerAngles.y, 0.0f);
            }
        }

        private bool DoHandle(int i, Transform knot)
        {
            var pointOnTerrain = TerrainUtility.GetPointOnTerrain(terrainList, knot.position);
            var heightAtKnot = pointOnTerrain.y;

            var dist = Mathf.Abs(knot.position.y - heightAtKnot) * 2.0f;
            var ease = dist;

            Handles.color = Color.green;
            Handles.DrawLine(knot.position, pointOnTerrain);
            Handles.DrawWireDisc(pointOnTerrain, Vector3.up, ease);

            EditorGUI.BeginChangeCheck();

            // const float fadeMin = 140.0f;
            // const float fadeMax = 250.0f;
            // const float alphaMin = 0.1f;

            var handleScale = HandleUtility.GetHandleSize(knot.position) * TrackSegmentTool.handleScale;

            // var screenPosition = (Vector2)Camera.current.WorldToScreenPoint(knot.transform.position);
            // var mousePosition = Mouse.current.position.ReadValue();
            // mousePosition = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            // var screenDistance = (screenPosition - mousePosition).magnitude;
            //
            // handleScale *= screenDistance < fadeMin ? 1.0f : (1.0f - alphaMin) * Mathf.Exp(-sqr(2.0f * (screenDistance - fadeMin) / (fadeMax - fadeMin))) + alphaMin;

            // ReSharper disable once ReplaceWithSingleAssignment.True
            var drawDiscs = true;
            if (i == 0 && trackSegment.StartConnection) drawDiscs = false;
            if (i == trackSegment.FromEnd(1) && trackSegment.EndConnection) drawDiscs = false;

            var newPosition = knot.position;
            var offset = Vector3.zero;

            setHandleColor(Handles.xAxisColor);
            newPosition = Handles.Slider(newPosition, knot.right, handleScale, Handles.ArrowHandleCap, 0.0f);
            setHandleColor(Handles.yAxisColor);
            newPosition = Handles.Slider(newPosition, Vector3.up, handleScale, Handles.ArrowHandleCap, 0.0f);
            newPosition = Handles.Slider2D(newPosition, Vector3.up, Vector3.right, Vector3.forward, handleScale * 0.3f, Handles.RectangleHandleCap, 0.0f);
            setHandleColor(Handles.zAxisColor);
            newPosition = Handles.Slider(newPosition, knot.forward, handleScale, Handles.ArrowHandleCap, 0.0f);

            var newRotation = knot.rotation;
            if (drawDiscs)
            {
                Handles.color = Handles.yAxisColor;
                newRotation = Handles.Disc(newRotation, knot.position, Vector3.up, handleScale * 1.5f, false, 0.0f);
                Handles.color = Handles.xAxisColor;
                newRotation = Handles.Disc(newRotation, knot.position, knot.right, handleScale * 1.5f, false, 0.0f);
            }

            if (!EditorGUI.EndChangeCheck()) return false;

            lastSelectedSegment = (trackSegment, i);

            if (Keyboard.current.leftShiftKey.isPressed)
            {
                var lines = new List<Ray>();
                var knots = trackSegment.KnotContainer();

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

            if (Keyboard.current.leftCtrlKey.isPressed)
            {
                newPosition.y = pointOnTerrain.y;
            }

            knot.position = newPosition + offset;
            knot.rotation = newRotation;

            return true;

            void setHandleColor(Color color) => Handles.color = color;
        }

        private void ModifyKnots(ModifyKnotCallback callback)
        {
            var knots = trackSegment.KnotContainer();
            var knotCount = knots.childCount;
            var dirty = false;

            if (knotMask == -1)
            {
                for (var i = 0; i < knotCount; i++)
                {
                    var knot = knots.GetChild(i);

                    Undo.RecordObject(knot, "Moved Track Knot");
                    if (!callback(i, knot)) continue;
                    dirty = true;
                }
            }
            else
            {
                var knot = knots.GetChild(knotMask);

                Undo.RecordObject(knot, "Moved Track Knot");
                if (callback(knotMask, knot))
                {
                    dirty = true;
                }
            }


            if (dirty) trackSegment.OnKnotsChanged();
        }

        private delegate bool ModifyKnotCallback(int i, Transform knot);
    }
}