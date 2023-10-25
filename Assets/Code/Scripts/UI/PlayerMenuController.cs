using System;
using FR8Runtime.CodeUtility;
using FR8Runtime.Player;
using FR8Runtime.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace FR8Runtime.UI
{
    public class PlayerMenuController : MonoBehaviour
    {
        [SerializeField] private InputAction pauseAction;
        [SerializeField] private VisualTreeAsset saveGroupTemplate;

        private PlayerAvatar avatar;

        private UIDocument[] menus;

        private int index;

        private const int PauseMenu = 0;
        private const int LoadMenu = 1;

        private void Start()
        {
            avatar = GetComponentInParent<PlayerAvatar>();

            pauseAction.performed += _ => ToggleMenu();
            pauseAction.Enable();

            menus = GetComponentsInChildren<UIDocument>();

            var pauseMenu = menus[PauseMenu];
            UIActions.BindButton(pauseMenu, "resume", HideMenu);
            UIActions.BindButton(pauseMenu, "reload-scene", HideAnd(UIActions.Load(SceneUtility.Scene.Game)));
            UIActions.BindButton(pauseMenu, "load-save", () => SetMenu(LoadMenu));
            UIActions.BindButton(pauseMenu, "return", HideAnd(UIActions.Load(SceneUtility.Scene.Menu)));
            UIActions.BindButton(pauseMenu, "quit", HideAnd(UIActions.QuitToDesktop));

            //UIActions.ClickSfx();

            BindBackButtons();
            HideMenu();
        }

        private Action HideAnd(Action callback) => () =>
        {
            HideMenu();
            callback?.Invoke();
        };

        private void OnEnable()
        {
            pauseAction.Enable();
        }

        private void OnDisable()
        {
            pauseAction.Disable();
        }

        private void OnDestroy()
        {
            pauseAction.Dispose();
        }

        private void BindBackButtons()
        {
            for (var i = 0; i < menus.Length; i++)
            {
                if (i == PauseMenu) continue;

                var back = menus[i].rootVisualElement.Q<Button>("back");
                if (back == null) continue;

                back.clickable.clicked += () => SetMenu(PauseMenu);
            }
        }

        public void ToggleMenu() => SetMenu(index == -1 ? 0 : -1);
        public void HideMenu() => SetMenu(-1);

        public void SetMenu(int index)
        {
            UIActions.ClickSfx();

            this.index = index;

            if (!avatar.IsAlive) this.index = -1;
            Cursor.lockState = CursorLockMode.None;

            for (var i = 0; i < menus.Length; i++)
            {
                menus[i].rootVisualElement.visible = i == index;
            }

            Pause.SetPaused(index != -1);
            
            if (index == LoadMenu) UpdateLoadMenu();
        }

        private Action LoadScene(SceneUtility.Scene scene) => () =>
        {
            HideMenu();
            SceneUtility.LoadScene(scene);
        };

        public Action LoadScene(string filename)
        {
            return () =>
            {
                HideMenu();
                SaveManager.saveName = filename;
                SaveManager.SlotSave.Load();
                SceneUtility.LoadScene(SceneUtility.Scene.Game);
            };
        }

        public void UpdateLoadMenu()
        {
            var root = menus[LoadMenu].rootVisualElement;
            LoadMenuController.ReloadList(this, root, saveGroupTemplate);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}