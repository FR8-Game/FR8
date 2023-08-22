using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace FR8Runtime.UI
{
    public static class UIUtility
    {
        public static UnityEngine.UI.Button ButtonPrefab { get; set; }
        
        public static void Button(Transform parent, string name, UnityAction callback)
        {
            var instance = Object.Instantiate(ButtonPrefab, parent);
            instance.transform.SetAsLastSibling();

            var title = instance.GetComponentInChildren<TMP_Text>();
            title.text = name;
            
            if (callback != null) instance.onClick.AddListener(callback);
        }
    }
}