using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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
        private TextElement content;
        private static string lastText;
        private static bool firstLoad = true;

        private bool loadingNewLevel;

        private const string LoadSpinnyThing = "|/-\\";

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;
            content = root.Q<TextElement>("content");
        }

        private IEnumerator Start()
        {
            if (firstLoad)
            {
                firstLoad = false;
                yield return StartCoroutine(DisplayLoadVisuals(0, FakeLoadScene));
            }
            
            content.text = $"{lastText}\n--- Loading Complete ---";
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(Fade(v => 1.0f - v));
        }

        private IEnumerator DisplayLoadVisuals(int buildIndex, Func<int, Action<float>, IEnumerator> loadCallback)
        {
            const int barWidth = 24;
            const string barFormat = "[#_]";

            var directory = Application.dataPath;
            var prepend = $"\n{directory}\\>";

            var lines = new List<string>();
            lines.Add($"FR8 OS [Version {Application.unityVersion}]");
            lines.Add($"(c) FR8 Corporation. All rights reserved.\n\n");

            var sceneName = $"{SceneManager.GetSceneByBuildIndex(buildIndex).name}.scene";

            apply();

            yield return StartCoroutine(startCommand($"scene-manager.exe -load-new {sceneName}", 1.0f));
            yield return StartCoroutine(appendOutput($"Loading scene-manager-v{Application.version}-stable", 0.2f));
            yield return StartCoroutine(appendOutput($"Scene \"{sceneName}\" was found, starting load operation..."));

            var finishedLoading = false;
            var t = 0.0f;
            yield return StartCoroutine(loadCallback(buildIndex, progress =>
            {
                t += Time.deltaTime;

                if (progress < 0.9f)
                {
                    var c0 = Mathf.FloorToInt(progress * barWidth);
                    var c1 = barWidth - c0 - 1;
                    var blink = t / 2.0f % 1.0f > 0.5f;

                    modifyLastLine($"{barFormat[0]}{new string(barFormat[1], c0)}{barFormat[blink ? 1 : 2]}{new string(barFormat[2], c1)}{barFormat[3]}");
                }
                else if (!finishedLoading)
                {
                    finishedLoading = true;
                    modifyLastLine($"{barFormat[0]}{new string(barFormat[1], barWidth)}{barFormat[3]}");
                    appendOutput($"Finished Loading {sceneName}");
                    appendOutput($"Initializing {sceneName}...");
                }
            }));

            IEnumerator startCommand(string command, float wait = 0.12f)
            {
                lines[^1] = $"{prepend}{command}";
                apply();
                yield return new WaitForSeconds(wait);
            }

            IEnumerator appendOutput(string output, float wait = 0.12f)
            {
                lines.Add(output);
                apply();
                yield return new WaitForSeconds(wait);
            }

            void modifyLastLine(string newLine)
            {
                lines[^1] = newLine;
                apply();
            }

            void apply()
            {
                content.text = string.Empty;
                foreach (var l in lines)
                {
                    content.text += $"{l}\n";
                }

                lastText = content.text;
            }
        }

        public void StartSceneLoad(int buildIndex)
        {
            StartCoroutine(routine());

            IEnumerator routine()
            {
                if (loadingNewLevel) yield break;
                loadingNewLevel = true;

                yield return StartCoroutine(Fade(v => v));
                yield return StartCoroutine(DisplayLoadVisuals(buildIndex, LoadScene));
                loadingNewLevel = false;
            }
        }

        private IEnumerator LoadScene(int buildIndex, Action<float> loadCallback)
        {
            var operation = SceneManager.LoadSceneAsync(buildIndex);
            while (!operation.isDone)
            {
                loadCallback(operation.progress);
                yield return null;
            }
        }
        
        private IEnumerator FakeLoadScene(int buildIndex, Action<float> loadCallback)
        {
            var step = 0;
            var steps = 5;
            var timer = 0.0f;
            var timerMax = 1.0f;
            
            while (step < steps)
            {
                if (timer > timerMax)
                {
                    timer -= timerMax;
                    step++;
                    timerMax = Random.Range(0.2f, 0.4f);
                }
                
                timer += Time.deltaTime;
                loadCallback(step / (float)steps);
                yield return null;
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
    }
}