using System;
using System.Collections;
using FR8Runtime.UI.CustomControls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace FR8Runtime.UI.Loading
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class LoadScreen : MonoBehaviour
    {
        [SerializeField] private float fadeTime = 0.5f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        private UIDocument document;
        private VisualElement root;
        private LoadBar loadBar;

        private bool loadingNewLevel;

        private const string LoadSpinnyThing = "|/-\\";
        
        private void Awake()
        {
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;
            loadBar = root.Q<LoadBar>("load-bar");
        }

        private void Start()
        {
            StartCoroutine(Fade(v => 1.0f - v));
        }

        private void Update()
        {
            if (root.visible)
            {
                loadBar.Prepend = $"[{LoadSpinnyThing[Mathf.FloorToInt(Time.time * 3.0f) % LoadSpinnyThing.Length]}] ";
            }
        }

        public void LoadScene(int buildIndex)
        {
            StartCoroutine(routine());

            IEnumerator routine()
            {
                if (loadingNewLevel) yield break;
                loadingNewLevel = true;

                SetFill(0.0f);
                
                yield return StartCoroutine(Fade(v => v));
                
                var operation = SceneManager.LoadSceneAsync(buildIndex);
                while (!operation.isDone)
                {
                    SetFill(operation.progress);
                    yield return null;
                }

                loadingNewLevel = false;
            }
        }

        private IEnumerator Fade(Func<float, float> remap)
        {
            root.visible = true;

            var p = 0.0f;
            while (p < 1.0f)
            {
                root.style.opacity = fadeCurve.Evaluate(remap(p));
                p += Time.deltaTime / fadeTime;
                yield return null;
            }

            root.style.opacity = fadeCurve.Evaluate(remap(1.0f));
            root.visible = root.style.opacity.value > 0.5f;
        }

        public void SetFill(float percent)
        {
            Debug.Log(percent);
            if (loadBar == null) return;
            loadBar.Percent = percent;
        }
    }
}