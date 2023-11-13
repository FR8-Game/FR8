using FMOD.Studio;
using FMODUnity;
using FR8.Runtime.CodeUtility;
using FR8.Runtime.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace FR8.Runtime.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        private UIDocument docs;
        [SerializeField] private FMOD.Studio.EventInstance titleMusic;
        [SerializeField] private EventReference MenuMusic;


        private void Awake()
        {
            docs = GetComponent<UIDocument>();
            titleMusic = RuntimeManager.CreateInstance(MenuMusic);
            titleMusic.start();
        }

        private void Start()
        {
            var root = docs.rootVisualElement;
            SetupLanding(root.Q("landing"));

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            ReloadSettings();
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
            root.Q<Button>("exit").clickable.clicked += UIActions.QuitToDesktop;
            root.Q<Button>().clickable.clicked += UIActions.ClickSfx;
        }
    }
}