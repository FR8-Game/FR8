using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FR8.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class ModalWindow : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button buttonPrefab;

        private Transform buttonContainer;
        private TMP_Text title;
        private TMP_Text content;
        private bool cancellable = true;

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

        private void Awake()
        {
            var window = transform.GetChild(1);

            title = window.GetChild(1).GetComponent<TMP_Text>();
            content = window.GetChild(2).GetComponent<TMP_Text>();
            buttonContainer = window.GetChild(3);
        }

        private void Start()
        {
            if (cancellable) UIUtility.Button(transform, "Cancel", () => Destroy(gameObject));
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
