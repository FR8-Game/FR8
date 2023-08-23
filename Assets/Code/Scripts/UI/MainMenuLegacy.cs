using UnityEngine;
using UnityEngine.SceneManagement;

namespace FR8Runtime.UI
{
    public class MainMenuLegacy : MonoBehaviour
    {
        public GameObject mainMenuCanvas;
        public GameObject mainMenuCamera;
        public GameObject settingsCanvas;
        public GameObject settingsCamera;

        #region Main Menu
        public void Play()
        {
            SceneManager.LoadScene("Depot");
        }

    public void SettingsMain()
        {
            mainMenuCamera.SetActive(false);
            mainMenuCanvas.SetActive(false);
            settingsCanvas.SetActive(true);
            settingsCamera.SetActive(true);
        }

        #endregion
        #region Settings Menu
        public void ReturnToMainMenu()
        {
            mainMenuCamera.SetActive(true);
            mainMenuCanvas.SetActive(true);
            settingsCanvas.SetActive(false);
            settingsCamera.SetActive(false);
        }

        #endregion
    }
}
