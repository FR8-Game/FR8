using FR8Runtime.CodeUtility;
using FR8Runtime.Save;
using FR8Runtime.References;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using FMODUnity;
using FMOD.Studio;

namespace FR8Runtime.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        private UIDocument docs;
        private EventInstance titleMusic;


        private void Awake()
        {
            docs = GetComponent<UIDocument>();
            titleMusic = SoundReference.TitleMusic.InstanceAndStart();
        }

        private void Start()
        {
            var root = docs.rootVisualElement;
            SetupLanding(root.Q("landing"));

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            ReloadSettings();
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
                SaveManager.PersistantSave.data = new PersistantSaveData();
                SaveManager.PersistantSave.Save();
                ReloadSettings();
            }
        }

        private void ReloadSettings()
        {
            SaveManager.PersistantSave.Load();
            var settings = SaveManager.PersistantSave.data;
            Screen.SetResolution(settings.xResolution, settings.yResolution, (FullScreenMode)settings.fullscreenMode);
        }

        private void SetupLanding(VisualElement root)
        {
            root.Q<Button>("start").clickable.clicked += UIActions.Load(SceneUtility.Scene.Game);
            root.Q<Button>("exit").clickable.clicked += UIActions.QuitToDesktop;
            root.Q<Button>().clickable.clicked += UIActions.ClickSfx;
        }
    }
}