using System.IO;
using FR8Runtime.CodeUtility;
using FR8Runtime.Player;
using FR8Runtime.Save;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FR8Runtime.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private InputAction pauseAction;
        [SerializeField] private float fadeTime = 0.1f;

        [Space]
        [SerializeField] private ModalWindow modalPrefab;

        private PlayerAvatar avatar;

        private CanvasGroup menuGroup;
        private CanvasGroup[] groups;

        private bool open;
        private float opacity;
        
        private float timeScale;
        private int cursorLockID;

        private const int MainGroup = 0;
        private const int LoadGroup = 1;
        private const int GroupCount = 2;

        private void Awake()
        {
            menuGroup = GetComponentInChildren<CanvasGroup>();
            avatar = GetComponentInParent<PlayerAvatar>();

            pauseAction.performed += _ => ToggleMenu();
            pauseAction.Enable();

            groups = new CanvasGroup[GroupCount];

            groups[MainGroup] = transform.Find("Main").GetComponent<CanvasGroup>();

            var buttonPrefab = groups[MainGroup].transform.Find("Button").GetComponent<Button>();

            UIUtility.MakeButtonList(buttonPrefab,
                ("Resume", HideMenu),
                ("Load Save", ChangeToLoadMenu),
                ("Quit To Menu", QuitModal)
            );

            groups[LoadGroup] = transform.Find("Load").GetComponent<CanvasGroup>();

            SetupBackButtons();
        }

        public void ToggleMenu() => SetMenu(!open);
        public void HideMenu() => SetMenu(false);

        public void SetMenu(bool state)
        {
            open = state;
            
            if (!avatar.IsAlive) open = false;
            Cursor.lockState = CursorLockMode.None;

            menuGroup.blocksRaycasts = open;
            menuGroup.interactable = open;
            
            Pause.SetPaused(open);

            if (open)
            {
                ChangeMenu(MainGroup);
            }
        }

        private void Update()
        {
            opacity += ((open ? 1.0f : 0.0f) - opacity) * (2.0f / fadeTime) * Time.unscaledDeltaTime;
            opacity = Mathf.Clamp01(opacity);
            menuGroup.alpha = CurvesUtility.SmootherStep(opacity);
        }

        private void OnDestroy()
        {
            pauseAction.Dispose();
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ChangeMenu(int i)
        {
            for (var j = 0; j < groups.Length; j++)
            {
                groups[j].gameObject.SetActive(i == j);
            }
        }
        
        private UnityAction ChangeMenuFunc(int i) => () => ChangeMenu(i);

        [ContextMenu("Test")]
        private void Test()
        {
            SaveManager.GetAllSaveGroups();
        }

        private void ChangeToLoadMenu()
        {
            var loadGroup = groups[LoadGroup];
            var saveGroups = SaveManager.GetAllSaveGroups();

            var templates = loadGroup.transform.Find("Scroll View/Templates");
            templates.gameObject.SetActive(false);

            var header = templates.Find("Header").GetComponent<TMP_Text>();
            var button = templates.Find("Button").GetComponent<Button>();

            var instances = loadGroup.transform.Find("Scroll View/Viewport/Content");
            for (var i = 0; i < instances.childCount; i++)
            {
                Destroy(instances.GetChild(i).gameObject);
            }
            
            foreach (var save in saveGroups)
            {
                var h = Instantiate(header, instances);
                h.text = save.saveName;
                
                foreach (var filename in save.filenames)
                {
                    var b = Instantiate(button, instances);
                    var name = Path.GetFileNameWithoutExtension(filename);
                    b.onClick.AddListener(LoadScene(filename));
                    b.GetComponentInChildren<TMP_Text>().text = name;
                }
            }

            ChangeMenu(LoadGroup);
        }

        public UnityAction LoadScene(string filename)
        {
            return () =>
            {
                SetMenu(false);
                SaveManager.LoadSave(filename);
            };
        }

        private void SetupBackButtons()
        {
            foreach (var group in groups)
            {
                var t = group.transform.Find("Back");
                if (!t) continue;

                var button = t.GetComponent<Button>();
                button.onClick.AddListener(() => ChangeMenu(MainGroup));
            }
        }
        
        private void QuitModal()
        {
            if (modalPrefab) modalPrefab.Create("Quit Game", "Are you sure you want to quit the game?", true, ("Quit", quit));
            else Debug.LogWarning($"Pause Menu is missing a ModalWindow prefab!", this);

            void quit()
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
        }
    }
}