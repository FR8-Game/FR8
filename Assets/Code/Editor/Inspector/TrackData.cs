using System;
using FR8.Track;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(TrackData))]
    public class TrackDataEditor : Editor
    {
        // private void OnEnable()
        // {
        //     SceneView.duringSceneGui += DrawSceneGUI;
        // }
        //
        // private void OnDisable()
        // {
        //     SceneView.duringSceneGui -= DrawSceneGUI;
        // }
        //
        // public override void OnInspectorGUI()
        // {
        //     base.OnInspectorGUI();
        //
        //     var trackData = target as TrackData;
        //     if (!trackData) return;
        //
        //     if (GUILayout.Button("Re-bake Track", GUILayout.Height(30)))
        //     {
        //         trackData.BakeTrack();
        //     }
        // }
    }
}