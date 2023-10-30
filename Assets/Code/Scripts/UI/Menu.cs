using System;
using System.Collections.Generic;
using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.UI
{
    [RequireComponent(typeof(UIDocument))]
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Menu : MonoBehaviour
    {
        protected UIDocument document;
        protected VisualElement root;

        public static readonly List<Menu> MenuStack = new();

        [ContextMenu("Push")]
        private void PushContentMenu() => Push(this);

        [ContextMenu("Pop")]
        private void PopContentMenu() => Pop();

        protected virtual void Awake()
        {
            document = GetComponent<UIDocument>();
            enabled = false;
        }

        protected virtual void OnEnable()
        {
            document.enabled = true;
            root = document.rootVisualElement;
            Setup();
        }

        protected virtual void OnDisable()
        {
            document.enabled = false;
        }

        public abstract void Setup();

        public static void Push(Menu menu)
        {
            MenuStack.Add(menu);
            ValidateStack();
        }

        public static void Pop()
        {
            if (MenuStack.Count > 0)
            {
                MenuStack[^1].enabled = false;
                MenuStack.RemoveAt(MenuStack.Count - 1);
            }

            ValidateStack();
        }

        public static void Clear()
        {
            foreach (var e in MenuStack)
            {
                e.enabled = false;
            }
            MenuStack.Clear();
            ValidateStack();
        }

        private static void ValidateStack()
        {
            MenuStack.RemoveAll(e => !e);
            for (var i = 0; i < MenuStack.Count; i++)
            {
                MenuStack[i].enabled = i == MenuStack.Count - 1;
            }

            Pause.SetPaused(MenuStack.Count > 0);
        }

        protected static Action Load(SceneUtility.Scene scene) => () =>
        {
            Clear();
            UIActions.Load(scene)();
        };
    }
}