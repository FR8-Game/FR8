using System.Collections.Generic;
using FR8.Train.Signals;
using UnityEngine;

namespace FR8.Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/Dialogue Chain")]
    public class DialogueChain : ScriptableObject
    {
        [SerializeField] private List<DialogueEntry> list;
        [SerializeField] private List<Signal> listeningSignals;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            listeningSignals.Listen(Queue);
        }

        public void Queue()
        {
            foreach (var e in list) DialogueListener.QueueDialogue(e);
        }
    }
}