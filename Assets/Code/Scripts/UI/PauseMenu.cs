using System;
using FR8Runtime.CodeUtility;
using FR8Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;

namespace FR8Runtime.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private InputAction pauseAction;

        [Space]
        [SerializeField] private Button buttonPrefab;
        [SerializeField] private ModalWindow modalPrefab;

        private PlayerAvatar avatar;
        private CanvasGroup group;

        private float timeScale;
        private int cursorLockID;

        private void Awake()
        {
            group = GetComponentInChildren<CanvasGroup>();
            avatar = GetComponentInParent<PlayerAvatar>();

            pauseAction.performed += _ => Pause.TogglePaused();
            pauseAction.Enable();

            UIUtility.MakeButtonList(buttonPrefab,
                ("Resume", Pause.TogglePaused),
                ("Reload Scene", ReloadScene),
                ("Quit To Menu", QuitModal)
            );

            OnPauseStateChanged();
        }

        private void OnDestroy()
        {
            pauseAction.Dispose();
        }

        private void OnEnable()
        {
            Pause.PausedStateChangedEvent += OnPauseStateChanged;
        }

        private void OnDisable()
        {
            Pause.PausedStateChangedEvent -= OnPauseStateChanged;
        }

        private void OnPauseStateChanged()
        {
            var paused = Pause.Paused;
            if (!avatar.IsAlive) paused = false;

            Cursor.lockState = CursorLockMode.None;
            
            group.alpha = paused ? 1.0f : 0.0f;
            group.blocksRaycasts = paused;
            group.interactable = paused;
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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