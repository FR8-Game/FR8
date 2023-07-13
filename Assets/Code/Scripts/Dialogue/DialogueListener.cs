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
            Debug.Log($"Queue.Count: {queue.Count}");
            if (!routineActive) StartCoroutine(ShowRoutine());
        }

        private void SoftPlayAnimation(string animation, string fallback, int layer = 0, Action<string> play = null)
        {
            if (play == null) play = name => animator.Play(name, layer);
            
            var hash = Animator.StringToHash(animation);
            play(animator.HasState(layer, hash) ? animation : fallback);
        }

        private IEnumerator ShowRoutine()
        {
            routineActive = true;

            while (queue.Count > 0)
            {
                var entry = queue.Dequeue();

                UpdateVisuals(entry);
                SoftPlayAnimation(entry.animateInOverride, "In");
                yield return StartCoroutine(Typewriter(entry));
                SoftPlayAnimation(entry.animateOutOverride, "Out");
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }

            routineActive = false;
        }

        private IEnumerator Typewriter(DialogueEntry entry)
        {
            for (var i = 0; i < entry.body.Length; i++)
            {
                if (skipDialogueAction.WasPerformedThisFrame()) i = entry.body.Length - 1;

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

            yield return new WaitForSeconds(additionalDelay);
        }

        private void UpdateVisuals(DialogueEntry entry)
        {
            headerDisplay.text = entry.source.title;
            bodyDisplay.text = entry.body;
            headshotDisplay.sprite = entry.source.headshot;
        }
    }
}