using System.Collections.Generic;
using System.Linq;
using FR8Runtime.Interactions.Drivers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(DriverNetwork))]
    public class DriverNetworkEditor : Editor<DriverNetwork>
    {
        public override bool RequiresConstantRepaint() => true;

        private VisualElement parent;
        private Dictionary<string, EntryTree> currentContainers = new();
        
        private void OnEnable()
        {
            Target.ValueChangedEvent += OnValueChanged;
        }

        private void OnDisable()
        {
            Target.ValueChangedEvent -= OnValueChanged;
        }

        public override void AddInspectorGUI(VisualElement root)
        {
            parent = new VisualElement();
            root.Add(parent);
        }

        public void BuildVisualTree()
        {
            currentContainers.Clear();
            parent.Clear();

            foreach (var v in Target.GetEnumerator())
            {
                currentContainers.Add(v.Key, new EntryTree(Target, v.Key, parent));
            }
        }
        
        private void OnValueChanged(string key, float value)
        {
            if (currentContainers.Count != Target.GetEnumerator().Count())
            {
                BuildVisualTree();
            }
            else
            {
                UpdateEntry(key);   
            }
        }

        private void UpdateEntry(string key)
        {
            currentContainers[key].Update();
        }

        private struct EntryTree
        {
            private DriverNetwork target;
            private string key;
            private FloatField value;
            private FloatField newValue;

            public EntryTree(DriverNetwork target, string key, VisualElement parent)
            {
                this.target = target;
                this.key = key;
                
                var root = new VisualElement();
                parent.Add(root);
                root.Add(value = new FloatField(key));

                root.Add(newValue = new FloatField());
                
                var submit = new Button();
                root.Add(submit);

                value.SetEnabled(false);
                submit.clickable.clicked += Submit;
                submit.text = "Override Value";
                Update();
            }

            public void Update()
            {
                value.value = target.GetValue(key);
                newValue.value = value.value;
            }
            
            private void Submit()
            {
                target.SetValue(key, newValue.value);
            }
        }
    }
}