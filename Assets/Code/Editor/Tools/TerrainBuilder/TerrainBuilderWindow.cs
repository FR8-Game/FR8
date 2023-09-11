using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace FR8Editor.Tools.TerrainBuilder
{
    public class TerrainBuilderWindow : EditorWindow
    {
        [SerializeField] private BuildPass buildPass = new ();
        
        [MenuItem("Tools/Terrain Builder")]
        public static void Open() => CreateWindow<TerrainBuilderWindow>("Terrain Builder");

        private void CreateGUI()
        {
            var root = rootVisualElement;

            CreateBuildSection(root);

            root.Bind(new SerializedObject(this));
        }

        private void CreateBuildSection(VisualElement parent)
        {
            var root = new VisualElement();
            parent.Add(root);

            var scroll = new ScrollView();
            root.Add(scroll);
            root = scroll;

            var so = new SerializedObject(this);
            var it = so.GetIterator();
            it.NextVisible(true);
            
            Section(root, so.FindProperty(nameof(buildPass)), buildPass.CreateGUI);
        }

        private void Section(VisualElement root, SerializedProperty property, Action<VisualElement> callback)
        {
            var container = new VisualElement();
            root.Add(container);
                
            container.style.marginTop = 6;
            container.style.marginBottom = 6;
            container.style.marginLeft = 6;
            container.style.marginRight = 6;
            container.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.2f);

            container.style.paddingBottom = 16;
            container.style.paddingTop = 16;
            container.style.paddingLeft = 16;
            container.style.paddingRight = 16;

            var label = new Label($"{property.displayName}");
            container.Add(label);
            label.style.fontSize = 18;
            label.style.borderBottomColor = Color.white;
            label.style.borderBottomWidth = 2.0f;

            var content = new VisualElement();
            container.Add(content);
            content.style.marginLeft = 20;
            
            callback(content);
        }

        private void Update()
        {
            buildPass.Update();
        }
    }
}