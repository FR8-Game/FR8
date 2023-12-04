using System;
using FMODUnity;
using FR8.Runtime.CodeUtility;
using FR8.Runtime.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace FR8.Runtime.UI
{
    public class MainMenu : MonoBehaviour
    {
        private UIDocument mainMenu;
        private UIDocument credits;
        [SerializeField] private FMOD.Studio.EventInstance titleMusic;
        [SerializeField] private EventReference MenuMusic;

        private void Awake()
        {
            mainMenu = transform.Find<UIDocument>("MainMenu");
            credits = transform.Find<UIDocument>("Credits");
            titleMusic = RuntimeManager.CreateInstance(MenuMusic);
            titleMusic.start();
        }

        private void Start()
        {
            var root = mainMenu.rootVisualElement;
            SetupLanding(root.Q("landing"));

            credits.rootVisualElement.Q<Button>("return").clickable.clicked += OpenCredits(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ReloadSettings();

            OpenCredits(false)();
        }

        private void OnDisable()
        {
            titleMusic.stop(STOP_MODE.ALLOWFADEOUT);
            titleMusic.release();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f3Key.isPressed && kb.sKey.wasPressedThisFrame)
            {
                ReloadSettings();
            }

            if (kb.f3Key.isPressed && kb.gKey.wasPressedThisFrame)
            {
                SaveManager.SettingsSave.data = new PersistantSaveData();
                SaveManager.SettingsSave.Save();
                ReloadSettings();
            }
        }

        private void ReloadSettings()
        {
            SaveManager.SettingsSave.Load();
            var settings = SaveManager.SettingsSave.data;
            Screen.SetResolution(settings.xResolution, settings.yResolution, (FullScreenMode)settings.displayMode);
        }

        private void SetupLanding(VisualElement root)
        {
            root.Q<Button>("start").clickable.clicked += UIActions.Load(SceneUtility.Scene.Game);
            root.Q<Button>("credits").clickable.clicked += OpenCredits(true);
            root.Q<Button>("exit").clickable.clicked += UIActions.QuitToDesktop;
            root.Q<Button>().clickable.clicked += UIActions.ClickSfx;
        }

        private Action OpenCredits(bool open) => () =>
        {
            mainMenu.rootVisualElement.visible = !open;
            credits.rootVisualElement.visible = open;
        };
    }
}