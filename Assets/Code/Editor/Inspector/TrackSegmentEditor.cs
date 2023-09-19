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
        public TrackSegment Segment => target as TrackSegment;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var connectionConfiguration = 0b0;
            if (Segment.StartConnection) connectionConfiguration |= 0b1 << 0;
            if (Segment.EndConnection) connectionConfiguration |= 0b1 << 1;

            EditorGUILayout.HelpBox(connectionConfiguration switch
            {
                0 => "Segment Has No Connections",
                1 => $"Connected at Start: [{Segment.StartConnection.segment.name}]",
                2 => $"Connected at End:   [{Segment.EndConnection.segment.name}]",
                3 => $"Connected at Start: [{Segment.StartConnection.segment.name}]\n" +
                     $"Connected at End:   [{Segment.EndConnection.segment.name}]",
                _ => throw new ArgumentOutOfRangeException()
            }, MessageType.Info);

            if (Button(Count("Zero Track Origin{0}")))
            {
                ExecuteAll(s => ShiftTransform(s, Vector3.zero, "Zero Track Origin"));
            }

            if (Button(Count("Center Track Origin{0}")))
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

            Div();

            if (Button("Add Knot", "d_Toolbar Plus"))
            {
                Segment.AddKnot();
            }

            Segment.UpdateKnotNames();
        }

        private bool Button(string name, string icon = null)
        {
            var content = icon == null ? new GUIContent(name) : new GUIContent(name, EditorGUIUtility.IconContent(icon).image);
            return Button(content);
        }

        private bool Button(GUIContent name) => GUILayout.Button(name, GUILayout.Height(30));

        private void Div()
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            rect.y += rect.height / 2.0f;
            rect.height = 1.0f;
            EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.1f));
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