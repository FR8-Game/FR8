using UnityEngine;
using UnityEngine.InputSystem;
using Button = FR8.Interactions.Button;

namespace FR8.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private InputAction pauseAction;
        
        [Space]
        [SerializeField] private Button buttonPrefab;

        private float timeScale;
        
        private void Awake()
        {
            pauseAction.performed += _ => TogglePause();
            pauseAction.Enable();
            
            gameObject.SetActive(false);
            
            UIUtility.Button(transform, "Resume", TogglePause);
            UIUtility.Button(transform, "Quit To Menu", null);
        }

        private void TogglePause()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void OnEnable()
        {
            Pause.Push();
        }

        private void OnDisable()
        {
            Pause.Pop();
        }
    }
}
