
using FR8Runtime.UI.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FR8Runtime.CodeUtility
{
    public static class SceneUtility
    {
        public static void LoadScene(Scene scene)
        {
            var loadScreen = Object.FindObjectOfType<LoadScreen>();

            if (loadScreen)
            {
                loadScreen.StartSceneLoad((int)scene);
            }
            else
            {
                SceneManager.LoadScene((int)scene);
            }
        }
        
        public static void Quit()
        {
#if !UNITY_EDITOR
            Application.Quit();
#else
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public enum Scene
        {
            Menu = 0,
            Game = 1,
        }
    }
}