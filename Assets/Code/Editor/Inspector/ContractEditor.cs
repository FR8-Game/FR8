using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FR8Runtime.Contracts;
using FR8Runtime.Contracts.Predicates;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(Contract))]
    public class ContractEditor : Editor<Contract>
    {
        private VisualElement root;

        private Type[] types;

        private void OnEnable()
        {
            types = typeof(ContractPredicate).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ContractPredicate))).ToArray();
        }

        public override void AddInspectorGUI(VisualElement root)
        {
            this.root = new VisualElement();
            root.Add(this.root);
            BuildInspector();
        }

        private void OnSceneGUI()
        {
            foreach (var e in Target.predicates)
            {
                var editor = CreateEditor(e);
                editor.GetType().GetMethod(nameof(OnSceneGUI), BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(editor, null);
            }
        }

        private void BuildInspector()
        {
            root.Clear();

            if (Target.predicates.Count > 0)
            {
                var container = Section("Predicates", root);

                foreach (var e in Target.predicates)
                {
                    var subContainer = Section(e.name, container);
                    var editor = CreateEditor(e);
                    var element = editor.CreateInspectorGUI() ?? new IMGUIContainer(editor.OnInspectorGUI);
                    subContainer.Add(element);

                    var delButton = new Button(DeletePredicate(e));
                    delButton.text = "<b>Delete Forever</b>";
                    delButton.style.height = 40;
                    delButton.style.marginTop = 15;
                    Border(delButton, Color.red, 3.0f, 3.0f);
                    subContainer.Add(delButton);
                }
            }

            foreach (var t in types)
            {
                var button = new Button();
                button.text = $"<b>Add {t.Name}</b>";
                button.clickable.clicked += CreatePredicate(t);
                button.style.height = 40;
                button.style.marginTop = 15;
                Border(button, Color.green, 3.0f, 3.0f);

                root.Add(button);
            }
        }

        private Action DeletePredicate(ContractPredicate predicate) => () =>
        {
            if (!EditorUtility.DisplayDialog("Delete Contract Predicate", $"Are you sure you want to delete \"{predicate.name}\"\nThis Cannot be Undone", "Delete Forever", "Cancel")) return;

            Target.predicates.Remove(predicate);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(predicate));
            DestroyImmediate(predicate);

            BuildInspector();
        };

        private Action CreatePredicate(Type type) => () =>
        {
            var predicate = (ContractPredicate)CreateInstance(type);
            predicate.name = $"{Target.name} Predicate.{Target.predicates.Count}";
            Target.predicates.Add(predicate);

            var dir = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(Target)), Target.name);
            Directory.CreateDirectory(dir);

            var filename = Path.Combine(dir, $"{predicate.name}.asset");
            AssetDatabase.CreateAsset(predicate, filename);

            BuildInspector();
            Repaint();
        };
    }
}