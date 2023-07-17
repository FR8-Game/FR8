using System;
using System.Collections;
using System.Collections.Generic;
using FR8.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Android;

namespace FR8.UI
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
            StartCoroutine(UITween.TweenIn(window, 0.2f, Vector2.down * 300.0f));
        }

        private void Hide()
        {
            IEnumerator routine()
            {
                yield return StartCoroutine(UITween.TweenOut(window, 0.2f, Vector2.down * 300.0f));
                Destroy(gameObject, 0.2f);
            }

            window.interactable = false;
            StartCoroutine(routine());
        }
    }
}
