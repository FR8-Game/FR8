using System;
using System.Collections.Generic;
using FR8Runtime.Train.Track;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Tools
{
    public class TrackTools : EditorWindow
    {
        [MenuItem("Tools/Track Tools")]
        public static void Open()
        {
            CreateWindow<TrackTools>("Track Tools");
        }

        private TrackJunction junctionPrefab;
        private Vector2 scrollPosition;

        private string actionOutputText = "Nothing To Report";
        private MessageType actionOutputType = MessageType.Info;

        private void OnGUI()
        {
            var margin = 10.0f;
            var reportAreaSize = EditorGUIUtility.singleLineHeight * 3.0f;
            var area = new Rect(margin, margin, position.width - margin * 2.0f, position.height - 2.0f * margin - reportAreaSize);

            EditorGUI.DrawRect(area, new Color(0.0f, 0.0f, 0.0f, 0.1f));

            area = new Rect(area.x + margin, area.y + margin, area.width - margin * 2.0f, area.height - margin * 2.0f);
            GUILayout.BeginArea(area);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                Section("Change Track Junction Prefab", () =>
                {
                    junctionPrefab = EditorGUILayout.ObjectField("Track Junction Prefab", junctionPrefab, typeof(TrackJunction), false) as TrackJunction;
                    using (new EditorGUI.DisabledScope(!junctionPrefab))
                    {
                        if (GUILayout.Button("Set Prefab on All Tracks", GUILayout.Height(30)) && junctionPrefab)
                        {
                            SetJunctionPrefabOnAllTracks();
                        }
                    }
                });
            }

            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(margin, position.height - margin * 2.0f - 30.0f, position.width - margin * 2.0f, reportAreaSize));

            EditorGUILayout.HelpBox(actionOutputText, actionOutputType);

            GUILayout.EndArea();
        }

        public bool Foldout(string label) => Foldout(label, EditorStyles.foldout);

        public bool Foldout(string label, GUIStyle style)
        {
            var key = $"{GetType().FullName}.{label}";

            var foldout = EditorPrefs.GetBool(key, false);
            foldout = EditorGUILayout.Foldout(foldout, label, true, style);
            EditorPrefs.SetBool(key, foldout);
            return foldout;
        }

        public void Section(string name, Action callback)
        {
            if (!Foldout(name, EditorStyles.foldoutHeader)) return;

            using (new EditorGUI.IndentLevelScope(EditorGUI.indentLevel + 1))
            {
                callback();
            }
        }

        private void SetJunctionPrefabOnAllTracks()
        {
            if (!junctionPrefab) return;

            var tracks = FindObjectsOfType<TrackSegment>();

            foreach (var t in tracks)
            {
                Undo.RecordObject(t, "Set Junction Prefab On All Tracks");
                t.junctionPrefab = junctionPrefab;
            }

            FinishAction($"Finished changed Prefabs on {tracks.Length} Junctions.", MessageType.Info);
        }

        public void FinishAction(string text, MessageType type)
        {
            actionOutputText = text;
            actionOutputType = type;

            Action<string> debugCallback = type switch
            {
                MessageType.None => Debug.Log,
                MessageType.Info => Debug.Log,
                MessageType.Warning => Debug.LogWarning,
                MessageType.Error => Debug.LogError,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            debugCallback(text);
        }
    }
}