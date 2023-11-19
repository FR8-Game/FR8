using FR8.Runtime.UI.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FR8.Runtime.CodeUtility
{
    public static class SceneUtility
    {
        public static void LoadScene(Scene scene) => LoadScene(scene == Scene.Reload ? SceneManager.GetActiveScene().buildIndex : (int)scene);
        public static void LoadScene(int index)
        {
            var loadScreen = Object.FindObjectOfType<LoadScreen>();

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