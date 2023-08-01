using FR8.Interactions.Drivers;
using TMPro;
using UnityEngine;

namespace FR8.Interactions.Drivables
{
    public class DriverDisplayText : MonoBehaviour, IDrivable
    {
        private string template;
        private TMP_Text text;
        
        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            template = text.text;
        }

        public void SetValue(DriverGroup group, float value)
        {
            text.text = string.Format(template, value, group.GroupName);
        }
    }
}
