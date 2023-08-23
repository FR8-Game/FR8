using System.Collections;
using FR8Runtime.CodeUtility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace FR8Runtime.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class ModalWindow : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button buttonPrefab;

        private Transform buttonContainer;
        private CanvasGroup window;
        private TMP_Text title;
        private TMP_Text content;

        public ModalWindow Create(string title, string content)
        {
            var instance = Instantiate(this);

            instance.title.text = title;
            instance.content.text = content;
            
            return instance;
        }

        public ModalWindow AddButton(string text, UnityAction callback)
        {
            UIUtility.ButtonPrefab = buttonPrefab;
            UIUtility.Button(buttonContainer, text, callback);
            return this;
        }

        public ModalWindow AddCancelButton(string text = "Cancel") => AddButton(text, Hide);

        private void Awake()
        {
            window = transform.GetChild(1).GetComponent<CanvasGroup>();

            title = window.transform.GetChild(1).GetComponent<TMP_Text>();
            content = window.transform.GetChild(2).GetComponent<TMP_Text>();
            buttonContainer = window.transform.GetChild(3);
        }

        private void OnEnable()
        {
            Pause.Push();
        }

        private void OnDisable()
        {
            Pause.Pop();
        }

        private void Start()
        {
            StartCoroutine(CodeUtility.UITween.BounceIn(window, 0.2f));
        }

        private void Hide()
        {
            IEnumerator routine()
            {
                yield return StartCoroutine(CodeUtility.UITween.BounceOut(window, 0.2f));
                Destroy(gameObject);
            }

            window.interactable = false;
            StartCoroutine(routine());
        }
    }
}
