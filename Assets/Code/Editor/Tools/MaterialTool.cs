using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Tools
{
    public class MaterialTool : EditorWindow
    {
        [MenuItem("Tools/Material Tool")]
        private static void Open() => CreateWindow<MaterialTool>("Material Tool");

        private List<Material> materials = new();
        private UnityEngine.Shader find;
        private UnityEngine.Shader replace;
        private Vector2 scrollPos;

        private void Find()
        {
            materials.Clear();
            var search = AssetDatabase.FindAssets("t:Material");
            foreach (var guid in search)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (mat.shader == find) materials.Add(mat);
            }
        }

        private void OnGUI()
        {
            find = (UnityEngine.Shader)EditorGUILayout.ObjectField("Find", find, typeof(UnityEngine.Shader), false);
            replace = (UnityEngine.Shader)EditorGUILayout.ObjectField("Replace", replace, typeof(UnityEngine.Shader), false);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.ExpandHeight(true)))
                {
                    scrollPos = scroll.scrollPosition;
                    var dead = new List<Material>();
                    foreach (var m in materials)
                    {
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorGUIUtility.singleLineHeight * 2.0f)))
                        {
                            if (GUILayout.Button(m.name, GUILayout.ExpandHeight(true))) EditorGUIUtility.PingObject(m);
                            if (GUILayout.Button("Remove", GUILayout.Width(160.0f), GUILayout.ExpandHeight(true))) dead.Add(m);
                        }
                    }

                    materials.RemoveAll(m => dead.Contains(m));
                }
            }

            if (GUILayout.Button("Execute"))
            {
                foreach (var m in materials)
                {
                    m.shader = replace;
                }
            }
            
            if (GUILayout.Button("Find")) Find();
        }
    }
}