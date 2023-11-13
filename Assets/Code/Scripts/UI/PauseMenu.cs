using System;
using FR8.Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace FR8.Runtime.UI
{
    public class PauseMenu : Menu
    {
        [SerializeField] private InputAction pauseAction;
        [SerializeField] private Menu[] otherMenus;

        protected override void Awake()
        {
            base.Awake();
            
            pauseAction.Enable();
            pauseAction.performed += OnPause;
        }

        private void OnDestroy()
        {
            pauseAction.performed -= OnPause;
        }

        private void OnPause(InputAction.CallbackContext obj)
        {
            if (enabled) Pop();
            else if (MenuStack.Count == 0) Push(this);
        }

        public override void Setup()
        {
            var content = root.Q("content");

            content.Add(Button("Resume", Pop));
            foreach (var e in otherMenus)
            {
                content.Add(Button(e.name, () => Push(e)));
            }
            content.Add(Button("Reload Scene", Load(SceneUtility.Scene.Game)));
            content.Add(Button("Return to Main Menu", Load(SceneUtility.Scene.Menu)));
            content.Add(Button("Exit To Desktop", UIActions.QuitToDesktop));
        }

        private VisualElement Button(string label, Action callback)
        {
            var button = new Button(callback);
            button.text = label.ToUpper();
            return button;
        }
    }
}