using System;
using FR8.UI.Loading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FR8.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button buttonInstance;

        private void Start()
        {
            makeButton("Play", LoadScene("Game Scene"));
            makeButton("Quit", Quit);
            
            Destroy(buttonInstance.gameObject);
            
            Button makeButton(string name, UnityAction callback)
            {
                var instance = Instantiate(buttonInstance, buttonInstance.transform.parent);
                instance.name = name;

                var text = instance.GetComponentInChildren<TMP_Text>();
                text.text = name;

                instance.onClick.AddListener(callback);
                return instance;
            }
        }

        public UnityAction LoadScene(string sceneName)
        {
            var loadScreen = FindObjectOfType<LoadScreen>();
            
            return () => loadScreen.LoadScene(sceneName);
        }

        public void Quit()
        {
#if !UNITY_EDITOR
            Application.Quit();
#else
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}