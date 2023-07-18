using TMPro;
using UnityEngine;

namespace FR8.Drivables
{
    public class DriverDisplay : MonoBehaviour, IDrivable
    {
        [SerializeField] private Vector2 displayRange = new(0.0f, 100.0f);
        [SerializeField] private string template = "{0:H1}%";
        
        private TMP_Text text;
        
        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        public void SetValue(float value)
        {
            var remappedValue = Mathf.Lerp(displayRange.x, displayRange.y, value);
            text.text = string.Format(template, remappedValue);
        }
    }
}
