using FR8;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrackPoint))]
    public class TrackPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var trackPoint = target as TrackPoint;
            if (!trackPoint) return;
            
            if (GUILayout.Button("Branch", GUILayout.Height(30)))
            {
                var instance = Instantiate(trackPoint, trackPoint.transform.position, trackPoint.transform.rotation, trackPoint.transform.parent);
                instance.name = "Track Point";
                instance.Connections.Clear();
                instance.Connections.Add(trackPoint);
                Selection.activeGameObject = instance.gameObject;
            }
        }
    }
}