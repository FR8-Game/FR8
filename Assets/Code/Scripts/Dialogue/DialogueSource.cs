using UnityEngine;

namespace FR8Runtime.Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/Dialogue Source")]
    public class DialogueSource: ScriptableObject
    {
        public string title;
        public Sprite headshot;
    }
}