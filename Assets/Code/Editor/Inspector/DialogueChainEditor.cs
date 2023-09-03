using FR8Runtime.Dialogue;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DialogueChain))]
    public class DialogueChainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying) EditorGUI.BeginDisabledGroup(true);
            
            if (GUILayout.Button("Queue"))
            {
                foreach (var t in targets)
                {
                    var dialogueChain = t as DialogueChain;
                    if (!dialogueChain) continue;
                    
                    dialogueChain.Queue();
                }
            }
            
            if (!Application.isPlaying) EditorGUI.EndDisabledGroup();
            
            base.OnInspectorGUI();
        }
    }
}