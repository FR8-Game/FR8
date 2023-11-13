using FR8.Runtime.Gamemodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrackSection))]
    public class TrackSectionEditor : Editor<TrackSection>
    {
        private void OnEnable()
        {
            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        public override void AddInspectorGUI(VisualElement root)
        {
            var button = new Button();
            button.text = "Snap to Spline";
            button.clickable.clicked += () =>
            {
                Undo.RecordObject(Target.transform, $"Snapped [{Target.name}] to Track Section");
                Target.SnapToSpline();
            };
            button.style.height = 40;

            root.Add(button);
        }
        
        private void OnSceneGUI()
        {
            var startScale = HandleUtility.GetHandleSize(Target.StartPosition);
            var endScale = HandleUtility.GetHandleSize(Target.EndPosition);
            
            Handles.Label(Target.StartPosition + Target.StartRotation * Vector3.back * startScale, "Start");
            Handles.Label(Target.EndPosition + Target.EndRotation * Vector3.forward * endScale * 2.0f, "End");

            var startPosition = Target.StartPosition;
            var startRotation = Target.StartRotation.normalized;
            
            var endPosition = Target.EndPosition;
            var endRotation = Target.EndRotation.normalized;
            
            switch (UnityEditor.Tools.current)
            {
                case Tool.Move:
                    startPosition = Handles.PositionHandle(startPosition, startRotation);
                    endPosition = Handles.PositionHandle(endPosition, endRotation);
                    break;
                case Tool.Rotate:
                    startRotation = Handles.RotationHandle(startRotation, startPosition);
                    endRotation = Handles.RotationHandle(endRotation, endPosition);
                    break;
            }
            
            if
            (
                startPosition == Target.StartPosition
                && startRotation == Target.StartRotation
                && endPosition == Target.EndPosition
                && endRotation == Target.EndRotation
            ) return;

            Undo.RecordObject(Target, $"Moved {Target.name} End Handle");
            Target.StartPosition = startPosition;
            Target.StartRotation = startRotation;
            Target.EndPosition = endPosition;
            Target.EndRotation = endRotation;
        }

        public void Update()
        {
            Target.UpdateEndsIfDirty();
        }
    }
}