using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FR8Runtime.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button buttonInstance;

        private void Start()
        {
            CodeUtility.UIUtility.MakeButtonList(buttonInstance,
                ("Play", LoadScene(SceneUtility.Scene.Game)),
                ("Quit", SceneUtility.Quit)
            );
        }

        public UnityAction LoadScene(SceneUtility.Scene scene)
        {
            return () => SceneUtility.LoadScene(scene);
        }
    }
}