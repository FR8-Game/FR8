using FR8.Train;
using UnityEditor;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrainMovement), true)]
    public class TrainMovementEditor : Editor
    {
        private bool SnapToSpline
        {
            get => EditorPrefs.GetBool($"{GetType().FullName}snapToSpline", false);
            set => EditorPrefs.SetBool($"{GetType().FullName}snapToSpline", value);
        }
        
        public TrainMovement Target => target as TrainMovement;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SnapToSpline = EditorGUILayout.Toggle("Snap To Spline", SnapToSpline);
            if (SnapToSpline && Target.Walker.CurrentSegment)
            {
                var transform = Target.transform;
                var track = Target.Walker.CurrentSegment;
                
                transform.position = track.SamplePoint(track.GetClosestPoint(transform.position));
            }
        }
    }
}