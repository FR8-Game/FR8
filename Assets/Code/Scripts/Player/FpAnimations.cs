using System;
using UnityEngine;

namespace FR8Runtime.Player
{
    [RequireComponent(typeof(Animator))]
    public class FpAnimations : MonoBehaviour
    {
        private Animator animator;

        private static event Action<string> PlayEvent;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            PlayEvent += OnPlay;
        }

        private void OnDisable()
        {
            PlayEvent -= OnPlay;
        }

        private void OnPlay(string animation)
        {
            animator.Play(animation, 0, 0.0f);
        }
        
        public static void Play(string animation)
        {
            PlayEvent?.Invoke(animation);
        }
    }
}
