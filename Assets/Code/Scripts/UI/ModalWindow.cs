using System.Collections;
using FR8.Runtime.CodeUtility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FR8.Runtime.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class ModalWindow : MonoBehaviour
    {
        private Button button;
        private CanvasGroup window;
        private TMP_Text title;
        private TMP_Text content;

        public ModalWindow Create(string title, string content, bool canCancel, params (string, UnityAction)[] def0)
        {
            var instance = Instantiate(this);

            instance.title.text = title;
            instance.content.text = content;

            if (canCancel)
            {
                var def1 = new (string, UnityAction)[def0.Length + 1];
                for (var i = 0; i < def0.Length; i++) def1[i] = def0[i];
                def1[^1] = ("Cancel", instance.Hide);

                UIUtility.MakeButtonList(instance.button, def1);
            }
            else
            {
                UIUtility.MakeButtonList(instance.button, def0);
            }

            return instance;
        }

        private void Awake()
        {
            window = FindUtility.Find<CanvasGroup>(transform, "Window");
            title = FindUtility.Find<TMP_Text>(transform, "Window/Title");
            content = FindUtility.Find<TMP_Text>(transform, "Window/Content");
            button = FindUtility.Find<Button>(transform, "Window/Buttons/Button");
        }

        private void OnEnable()
        {
            Pause.SetPaused(true);
        }

        private void OnDisable()
        {
            Pause.SetPaused(false);
        }

        private void Start()
        {
            StartCoroutine(UITween.BounceIn(window, 0.2f));
        }

        private void Hide()
        {
            IEnumerator routine()
            {
                yield return StartCoroutine(UITween.BounceOut(window, 0.2f));
                Destroy(gameObject);
            }

            window.interactable = false;
            StartCoroutine(routine());
        }
    }
}