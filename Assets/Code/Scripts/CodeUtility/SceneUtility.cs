
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

            var index = scene == Scene.Reload ? SceneManager.GetActiveScene().buildIndex : (int)scene;

            if (loadScreen)
            {
                loadScreen.StartSceneLoad(index);
            }
            else
            {
                SceneManager.LoadScene(index);
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
            
            Reload = 2,
        }
    }
}