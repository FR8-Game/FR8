using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FR8.UI.Loading
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class LoadScreen : MonoBehaviour
    {
        [SerializeField] private float fadeTime = 0.5f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        [Space]
        [SerializeField] private RectTransform content;

        [SerializeField] private float contentBobFrequency;
        [SerializeField] private float contentBobAmplitude;
        [SerializeField] private Image fill;

        private Canvas canvas;
        private CanvasGroup group;

        private bool loadingNewLevel;

        private static bool started;

        private void Awake()
        {
            canvas = GetComponentInChildren<Canvas>();
            group = canvas.GetComponentInChildren<CanvasGroup>();
        }

        private void Start()
        {
            group.blocksRaycasts = false;

            if (!started)
            {
                group.alpha = 0.0f;
                started = true;
                return;
            }

            StartCoroutine(Fade(v => 1.0f - v));
        }

        private void Update()
        {
            content.anchoredPosition = Vector3.up * Mathf.Sin(Time.time * contentBobFrequency * Mathf.PI) * contentBobAmplitude;
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(routine());

            IEnumerator routine()
            {
                if (loadingNewLevel) yield break;
                loadingNewLevel = true;

                group.blocksRaycasts = true;
                fill.fillAmount = 0.0f;

                yield return StartCoroutine(Fade(v => v));

                var operation = SceneManager.LoadSceneAsync(sceneName);
                while (!operation.isDone)
                {
                    fill.fillAmount = operation.progress;
                    yield return null;
                }

                loadingNewLevel = false;
            }
        }

        private IEnumerator Fade(Func<float, float> remap)
        {
            var p = 0.0f;
            while (p < 1.0f)
            {
                group.alpha = fadeCurve.Evaluate(remap(p));

                p += Time.deltaTime / fadeTime;
                yield return null;
            }

            group.alpha = fadeCurve.Evaluate(remap(1.0f));
        }
    }
}