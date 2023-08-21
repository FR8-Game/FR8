using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace FR8.UI.Loading
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
        private VisualElement fill;

        private bool loadingNewLevel;
        private static bool started;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
        }

        private void Start()
        {
            if (!started)
            {
                ShowUI(false);
                started = true;
                return;
            }

            StartCoroutine(Fade(v => 1.0f - v, () => root.SetEnabled(false)));
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(routine());

            IEnumerator routine()
            {
                ShowUI(true);
                
                if (loadingNewLevel) yield break;
                loadingNewLevel = true;
                
                fill.style.width = 0.0f;

                yield return StartCoroutine(Fade(v => v, null));

                var operation = SceneManager.LoadSceneAsync(sceneName);
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

            p = 0.0f;
            while (p < 2.0f)
            {
                SetFill(p < 1.0f ? p : 2.0f - p);
                p += Time.deltaTime / 10.0f;
                yield return null;
            }

            root.style.opacity = fadeCurve.Evaluate(remap(1.0f));
            finishedCallback?.Invoke();
        }

        public void SetFill(float percent)
        {
            if (fill == null) return;
            fill.style.width = percent;
        }
        
        public void ShowUI(bool state)
        {
            document.enabled = state;
            root = state ? document.rootVisualElement.Q("LoadingScreen") : null;
            fill = state ? document.rootVisualElement.Q("Fill") : null;
        }
    }
}