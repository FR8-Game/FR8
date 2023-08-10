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

        public static DialogueChain CreateFromString(string[] list, string headerSeparator, DialogueSource[] sourceList)
        {
            var instance = CreateInstance<DialogueChain>();

            foreach (var text in list)
            {
                var entry = new DialogueEntry();
                
                var splitPoint = text.IndexOf(headerSeparator);
                var header = text[..splitPoint];
                var body = text[splitPoint..];
                
                var source = sourceList[Utility.Search.FuzzySearch(i => sourceList[i].title, sourceList.Length, header)];
                entry.source = source;
                entry.body = body;
                
                instance.list.Add(entry);
            }

            return instance;
        }
    }
}