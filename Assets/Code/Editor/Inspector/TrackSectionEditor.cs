
using FR8Runtime.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrackSection))]
    public class TrackSectionEditor : Editor<TrackSection>
    {
        public override void AddInspectorGUI(VisualElement root)
        {
            var button = new Button();
            button.text = "Snap to Spline";
            button.clickable.clicked += () =>
            {
                Undo.RecordObject(Target.transform, $"Snapped Track Section[{Target.name}] to Spline[{Target.Track.name}]");
                Target.SnapToSpline();
            };
            button.style.height = 40;
            
            root.Add(button);
        }
    }
}