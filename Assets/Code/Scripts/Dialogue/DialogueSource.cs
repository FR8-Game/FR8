using UnityEngine;

namespace FR8.Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/Dialogue Source")]
    public class DialogueSource: ScriptableObject
    {
        public string title;
        public Sprite headshot;
    }
}