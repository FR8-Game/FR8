using System.Reflection;
using FR8Runtime.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(Contract))]
    public class ContractEditor : Editor<Contract>
    {
        private VisualElement root;

        public override void AddInspectorGUI(VisualElement root)
        {
            this.root = new VisualElement();
            root.Add(this.root);
            BuildInspector();
        }

        private void OnSceneGUI()
        {
            foreach (var e in Target.PredicateTree)
            {
                var editor = CreateEditor(e);
                editor.GetType().GetMethod(nameof(OnSceneGUI), BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(editor, null);
            }
        }

        private void BuildInspector()
        {
            root.Clear();

            if (Target.PredicateTree.Count <= 0)
            {
                var helpBox = new HelpBox("Contract Has No Predicates\n\nTo Get Started, add a child object with a Contract Predicate Component [Found in AddComponent::Contracts/Predicates/]", HelpBoxMessageType.Info);
                root.Add(helpBox);
                return;
            }

            foreach (var e in Target.PredicateTree)
            {
                var section = new VisualElement();
                section.AddToClassList("unity-help-box");
                section.style.marginTop = 6;
                
                var foldout = new Foldout();
                foldout.text = Application.isPlaying ? $"{e.name}[{e.Progress * 100.0f:N0}%]" : e.name;
                foldout.style.flexGrow = 1.0f;
                foldout.style.flexShrink = 0.0f;
                foldout.style.marginLeft = 16;
                foldout.style.unityFontStyleAndWeight = FontStyle.Bold;
                
                var editor = CreateEditor(e);
                var element = editor.CreateInspectorGUI() ?? new IMGUIContainer(() => editor.DrawDefaultInspector());
                element.style.marginTop = 6;
                element.style.marginBottom = 6;
                element.style.marginRight = 6;
                
                foldout.Add(element);
                section.Add(foldout);
                root.Add(section);
            }
        }
    }
}