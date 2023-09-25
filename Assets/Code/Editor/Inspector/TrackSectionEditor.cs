using System;
using FR8Runtime.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
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
            var startScale = HandleUtility.GetHandleSize(Target.transform.position);
            var endScale = HandleUtility.GetHandleSize(Target.endPosition);
            
            Handles.Label(Target.transform.position - Target.transform.forward * startScale, "Start");
            Handles.Label(Target.endPosition + Target.endRotation * Vector3.forward * endScale * 2.0f, "End");
            
            var endPosition = Target.endPosition;
            var endRotation = Target.endRotation.normalized;
            
            switch (UnityEditor.Tools.current)
            {
                case Tool.Move:
                    endPosition = Handles.PositionHandle(endPosition, endRotation);
                    break;
                case Tool.Rotate:
                    endRotation = Handles.RotationHandle(endRotation, endPosition);
                    break;
                case Tool.Rect:
                    break;
                default:
                case Tool.View:
                case Tool.Custom:
                case Tool.None:
                    break;
            }
            
            if
            (
                endPosition == Target.endPosition
                && endRotation == Target.endRotation
            ) return;

            Undo.RecordObject(Target, $"Moved {Target.name} End Handle");
            Target.endPosition = endPosition;
            Target.endRotation = endRotation;
        }

        public void Update()
        {
            Target.UpdateEndsIfDirty();
        }
    }
}