using TMPro;
using UnityEngine;

namespace FR8.Interactions.Drivables
{
    [System.Serializable]
    public class DriverDisplayText
    {
        [SerializeField] private TMP_Text text;
        
        private string template;
     
        public void Awake()
        {
            template = text.text;
        }
        
        public void SetValue(float newValue)
        {
            if (!text) return;
            text.text = string.Format(template, newValue);
        }
    }
}
