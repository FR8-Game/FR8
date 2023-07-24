using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SceneQuickAccess.Editor
{
    public class SceneQuickAccessWindow : EditorWindow
    {
        private const int ButtonHeight = 40;

        private Vector2 scrollPosition;

        private bool refreshScenes;
        private string search;
        private List<(SceneAsset, string)> cache;

        [MenuItem("Tools/Scene Quick Access/Scene Quick Access Window")]
        public static void OpenWindow()
        {
            CreateWindow<SceneQuickAccessWindow>("Scene Quick Access");
        }

        private void OnGUI()
        {
            var sceneIcon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image;
            var refreshIcon = EditorGUIUtility.IconContent("Refresh").image;
            GUIContent content;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scroll.scrollPosition;

                var scenes = GetScenes();
                foreach (var pair in scenes)
                {
                    var scene = pair.Item1;
                    var path = pair.Item2;

                    content = new GUIContent($"Open {scene.name}", sceneIcon, $"Open {scene.name}");

                    if (!GUILayout.Button(content, GUILayout.Height(ButtonHeight))) continue;

                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(path);
                    }
                }
            }

            var newSearch = EditorGUILayout.TextField("Search", search);
            if (newSearch != search)
            {
                refreshScenes = true;
            }

            search = newSearch;

            content = new GUIContent("Refresh", refreshIcon);
            if (!GUILayout.Button(content, GUILayout.Height(ButtonHeight))) return;
            refreshScenes = true;
            Repaint();
        }

        private List<(SceneAsset, string)> GetScenes()
        {
            if (cache == null) cache = new List<(SceneAsset, string)>();
            else if (!refreshScenes) return cache;

            refreshScenes = false;
            scrollPosition = Vector2.zero;

            var guids = AssetDatabase.FindAssets("t:scene");
            cache.Clear();

            var validityRegex = new Regex(@"[/\\]?Assets[/\\].+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!validityRegex.IsMatch(path)) continue;

                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                cache.Add((asset, path));
            }

            if (string.IsNullOrWhiteSpace(search)) return cache;

            var searchRegex = new Regex(search, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (cache.Exists(e => score(e, searchRegex) > 0))
            {
                cache.RemoveAll(e => score(e, searchRegex) == 0);
                cache.Sort((a, b) =>
                {
                    var scoreA = score(a, searchRegex);
                    var scoreB = score(b, searchRegex);
                    return scoreB - scoreA;
                });
            }

            return cache;

            int score((SceneAsset, string) pair, Regex regex)
            {
                return regex.Matches(pair.Item2).Count;
            }
        }
    }
}