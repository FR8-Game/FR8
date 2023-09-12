using System;
using UnityEngine;
using UnityEngine.UIElements;
using HBCore.Utility;
using ColorUtility = HBCore.Utility.ColorUtility;
using Object = UnityEngine.Object;

namespace FR8Editor.Inspector
{
    public abstract class Editor<T> : UnityEditor.Editor where T : Object
    {
        public T Target => (T)target;
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new IMGUIContainer(OnInspectorGUI));

            AddInspectorGUI(root);

            return root;
        }

        public virtual void AddInspectorGUI(VisualElement root) { }

        public VisualElement Section(string title, VisualElement root)
        {
            var container = new Foldout();
            
            root.Add(container);
            
            container.text = $"<b>{title}</b>";
            container.style.fontSize = 13;
            
            container.style.marginBottom = 10;

            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;

            container.style.borderTopColor = ColorUtility.Gray(1.0f, 0.2f);
            container.style.borderTopWidth = 1.0f;
            
            container.style.borderBottomColor = container.style.borderTopColor;
            container.style.borderBottomWidth = container.style.borderTopWidth;

            container.style.backgroundColor = ColorUtility.Gray(1.0f, 0.02f);
            
            var content = new VisualElement();
            container.Add(content);
            content.style.marginTop = 15;

            return content;
        }

        protected static void Margin(VisualElement target, int margin)
        {
            target.style.marginTop = margin;
            target.style.marginBottom = margin;
            target.style.marginLeft = margin;
            target.style.marginRight = margin;
        }

        protected static void Pad(VisualElement target, int pad)
        {
            target.style.paddingTop = pad;
            target.style.paddingBottom = pad;
            target.style.paddingLeft = pad;
            target.style.paddingRight = pad;
        }
        protected static void Border(VisualElement target, Color color, float width, float radius)
        {
            target.style.borderTopColor = color;
            target.style.borderBottomColor = color;
            target.style.borderLeftColor = color;
            target.style.borderRightColor = color;
            
            target.style.borderTopWidth = width;
            target.style.borderBottomWidth = width;
            target.style.borderLeftWidth = width;
            target.style.borderRightWidth = width;
            
            target.style.borderTopLeftRadius = radius;
            target.style.borderBottomLeftRadius = radius;
            target.style.borderTopRightRadius = radius;
            target.style.borderBottomRightRadius = radius;
        }
    }
}