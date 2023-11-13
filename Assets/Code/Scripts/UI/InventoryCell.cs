using FR8.Runtime.CodeUtility;
using FR8.Runtime.Pickups;
using TMPro;
using UnityEngine;

namespace FR8.Runtime.UI
{
    [SelectionBase, DisallowMultipleComponent]
    public class InventoryCell : MonoBehaviour
    {
        private CanvasGroup selectedGroup;
        private TMP_Text text;

        private void Awake()
        {
            selectedGroup = FindUtility.Find<CanvasGroup>(transform, "Selected");
            text = FindUtility.Find<TMP_Text>(transform, "Text");
            
            SetSelected(false);
            SetItem(null);
        }

        public void SetSelected(bool state)
        {
            selectedGroup.alpha = state ? 1.0f : 0.0f;
        }
        
        public void SetItem(PickupObject pickup)
        {
            text.text = pickup ? pickup.displayName : string.Empty;
        }
    }
}