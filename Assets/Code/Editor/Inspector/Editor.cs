using UnityEngine;
using UnityEngine.UIElements;

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

        public void Section(string title, VisualElement root, VisualElement content)
        {
            var container = new VisualElement();
            root.Add(container);

            Margin(container, 8);
            Pad(container, 8);
            container.style.color = new Color(0.0f, 0.0f, 0.0f, 0.2f);

            var titleElement = new Label(title);
            titleElement.style.fontSize = 18;
            container.Add(titleElement);

            container.Add(content);
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
    }
}