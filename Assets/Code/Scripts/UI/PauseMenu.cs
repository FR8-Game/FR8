using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;
using Cursor = FR8Runtime.CodeUtility.Cursor;

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

        private float timeScale;
        private int cursorLockID;

        private void Awake()
        {
            pauseAction.performed += _ => TogglePause();
            pauseAction.Enable();

            gameObject.SetActive(false);

            UIUtility.ButtonPrefab = buttonPrefab;

            UIUtility.Button(transform, "Resume", TogglePause);
            UIUtility.Button(transform, "Reload Scene", ReloadScene);
            UIUtility.Button(transform, "Quit To Menu", QuitModal);
        }

        private void OnDestroy()
        {
            pauseAction.Dispose();
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

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
