using UnityEngine;
using UnityEngine.InputSystem;
using Button = UnityEngine.UI.Button;
using Cursor = FR8.Utility.Cursor;

namespace FR8.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private InputAction pauseAction;

        [Space]
        [SerializeField] private Button buttonPrefab;

        [SerializeField] private ModalWindow modalPrefab;

        private float timeScale;
        private int cursorLockID;

        private void Awake()
        {
            pauseAction.performed += _ => TogglePause();
            pauseAction.Enable();

            gameObject.SetActive(false);

            UIUtility.ButtonPrefab = buttonPrefab;

            UIUtility.Button(transform, "Resume", TogglePause);
            UIUtility.Button(transform, "Quit To Menu", QuitModal);
        }

        private void TogglePause()
        {
            var paused = !gameObject.activeSelf;
            gameObject.SetActive(paused);

            if (paused)
            {
                Cursor.Push(CursorLockMode.None, ref cursorLockID);
            }
            else
            {
                Cursor.Pop(ref cursorLockID);
            }
        }

        private void OnEnable()
        {
            Pause.Push();
        }

        private void OnDisable()
        {
            Pause.Pop();
        }

        private void QuitModal()
        {
            if (modalPrefab) modalPrefab.Create("Quit Game", "Are you sure you want to quit the game?").AddButton("Quit", quit).AddCancelButton();
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