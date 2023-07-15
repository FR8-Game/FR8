using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FR8.Dialogue
{
    public class DialogueListener : MonoBehaviour
    {
        [SerializeField] private int typewriterCharactersPerSecond = 30;
        [SerializeField] private float pauseLength = 0.5f;
        [SerializeField] private float additionalDelay = 3.0f;

        [Space]
        [SerializeField] private InputAction skipDialogueAction;

        private bool skip;
        private bool canSkip;
        private Animator animator;
        private Image headshotDisplay;
        private TMP_Text headerDisplay;
        private TMP_Text bodyDisplay;

        private bool routineActive;
        private readonly Queue<DialogueEntry> queue = new();

        private static readonly HashSet<DialogueListener> all = new();

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            
            headshotDisplay = this.DeepFind<Image>("Headshot");
            headerDisplay = this.DeepFind<TMP_Text>("Header");
            bodyDisplay = this.DeepFind<TMP_Text>("Body");

            skipDialogueAction.Enable();
        }

        private void OnEnable()
        {
            all.Add(this);
            animator.Play("Out", 0, 1.0f);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        private void Update()
        {
            if (!canSkip) skip = false;
            else if (skipDialogueAction.WasPerformedThisFrame()) skip = true;
        }

        public static void QueueDialogue(DialogueEntry entry)
        {
            foreach (var e in all)
            {
                e.Queue(entry);
            }
        }

        private void Queue(DialogueEntry entry)
        {
            queue.Enqueue(entry);
            if (!routineActive) StartCoroutine(RunQueueRoutine());
        }

        private void SoftPlayAnimation(string animation, string fallback, int layer = 0, Action<string> play = null)
        {
            if (play == null) play = name => animator.Play(name, layer);
            
            var hash = Animator.StringToHash(animation);
            play(animator.HasState(layer, hash) ? animation : fallback);
        }

        private IEnumerator RunQueueRoutine()
        {
            routineActive = true;

            while (queue.Count > 0)
            {
                var entry = queue.Dequeue();
                if (entry == null) continue;
                
                yield return StartCoroutine(ShowEntryRoutine(entry));
            }

            routineActive = false;
        }

        private IEnumerator ShowEntryRoutine(DialogueEntry entry)
        {
            UpdateVisuals(entry);
            SoftPlayAnimation(entry.animateInOverride, "In");
            yield return StartCoroutine(Typewriter(entry));
            SoftPlayAnimation(entry.animateOutOverride, "Out");
            yield return StartCoroutine(PostDelay());
        }
        
        private IEnumerator Typewriter(DialogueEntry entry)
        {
            canSkip = true;
            
            for (var i = 0; i < entry.body.Length; i++)
            {
                if (skip)
                {
                    i = entry.body.Length - 1;
                    skip = false;
                }

                bodyDisplay.maxVisibleCharacters = i + 1;
                
                var delay = 1.0f / typewriterCharactersPerSecond + entry.body[i] switch
                {
                    ',' => pauseLength * 0.5f,
                    '.' => pauseLength,
                    '\n' => pauseLength,
                    _ => 0.0f,
                };
                yield return new WaitForSeconds(delay / entry.printSpeedMultiplier);
            }

            var timer = 0.0f;
            while (timer < additionalDelay)
            {
                if (skip)
                {
                    skip = false;
                    break;
                }
                
                timer += Time.deltaTime;
                yield return null;
            }

            canSkip = false;
        }

        private IEnumerator PostDelay()
        {
            var time = 0.0f;
            var totalTime = animator.GetCurrentAnimatorStateInfo(0).length;
            while (time < totalTime)
            {
                if (skip) yield break;
                
                time += Time.deltaTime;
                yield return null;
            }
        }

        private void UpdateVisuals(DialogueEntry entry)
        {
            if (headerDisplay) headerDisplay.text = entry.source.title;
            if (bodyDisplay) bodyDisplay.text = entry.body;
            if (headshotDisplay) headshotDisplay.sprite = entry.source.headshot;
            
            if (!headerDisplay) Debug.LogError("DialogueListener is missing a Header Display", this);
            if (!bodyDisplay) Debug.LogError("DialogueListener is missing a Body Display", this);
            if (!headshotDisplay) Debug.LogError("DialogueListener is missing a Headshot Display", this);
        }
    }
}