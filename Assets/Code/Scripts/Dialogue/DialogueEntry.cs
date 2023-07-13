using UnityEngine;

namespace FR8.Dialogue
{
    [System.Serializable]
    public class DialogueEntry
    {
        public DialogueSource source;
        [TextArea] public string body = "Placeholder Body";
        public float printSpeedMultiplier = 1.0f;

        public string animateInOverride;
        public string animateOutOverride;
    }
}