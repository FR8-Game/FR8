using System;
using FR8Runtime.CodeUtility;
using UnityEngine.UIElements;

namespace FR8Runtime.UI
{
    public static class UIActions
    {
        public static Action Load(SceneUtility.Scene scene) => () =>
        {
            Pause.SetPaused(false);
            SceneUtility.LoadScene(scene);
        };

        public static void QuitToDesktop() => SceneUtility.Quit();

        public static void BindButton(UIDocument document, string q, Action callback)
        {
            if (document.rootVisualElement == null) return;
            BindButton(document.rootVisualElement, q, callback);
        }
        
        public static void BindButton(VisualElement root, string q, Action callback)
        {
            var element = root.Q<Button>(q);
            if (element == null) return;
            element.clickable.clicked += callback;
        }
    }
}