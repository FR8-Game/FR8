using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class ControlsTextFactory : MonoBehaviour
    {
        [SerializeField] private int headerScale = 250;
        [SerializeField] private string heading = "Controls";
        [SerializeField][Range(0.0f, 100.0f)] private int offset = 60;
        [SerializeField] private int indent = 40;
        [SerializeField][TextArea] private string controls;
        
        private void OnValidate()
        {
            var textDisplay = GetComponent<TMP_Text>();
            if (!textDisplay) return;

            var sb = new StringBuilder();

            sb.Append("<size=").Append(headerScale).Append("%=>").Append(heading).Append("</size>");
            sb.Append("\n<indent=").Append(indent).Append("px>");

            foreach (var control in controls.Split('\n'))
            {
                var pair = control.Split(',');

                var first = pair.Length > 0 ? pair[0] : string.Empty;
                var second = pair.Length > 1 ? pair[1] : string.Empty;
                
                sb.Append(first).Append("<pos=").Append(offset).Append("%>| ").Append(second).Append("\n");
            }
            
            textDisplay.text = sb.ToString();
        }
    }
}
