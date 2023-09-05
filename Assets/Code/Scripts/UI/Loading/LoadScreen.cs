using System;
using System.Collections;
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

        [SerializeField] private float contentBobFrequency;
        [SerializeField] private float contentBobAmplitude;

        private UIDocument document;
        private VisualElement root;
        private ProgressBar fill;

        private bool loadingNewLevel;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
        }

        private void Start()
        {
            StartCoroutine(Fade(v => 1.0f - v, () => root.SetEnabled(false)));
        }

        public void LoadScene(int buildIndex)
        {
            StartCoroutine(routine());

            IEnumerator routine()
            {
                ShowUI(true);
                
                if (loadingNewLevel) yield break;
                loadingNewLevel = true;

                SetFill(0.0f);
               
                yield return StartCoroutine(Fade(v => v, null));

                var operation = SceneManager.LoadSceneAsync(buildIndex);
                while (!operation.isDone)
                {
                    SetFill(operation.progress);
                    yield return null;
                }

                loadingNewLevel = false;
            }
        }

        private IEnumerator Fade(Func<float, float> remap, Action finishedCallback)
        {
            ShowUI(true);

            var p = 0.0f;
            while (p < 1.0f)
            {
                root.style.opacity = fadeCurve.Evaluate(remap(p));
                p += Time.deltaTime / fadeTime;
                yield return null;
            }

            root.style.opacity = fadeCurve.Evaluate(remap(1.0f));
            finishedCallback?.Invoke();
        }

        public void SetFill(float percent)
        {
            if (fill == null) return;
            fill.value = percent;
        }
        
        public void ShowUI(bool state)
        {
            document.enabled = state;
            root = state ? document.rootVisualElement.Q("loading-screen") : null;
            fill = state ? document.rootVisualElement.Q<ProgressBar>("progress-bar") : null;
        }
    }
}