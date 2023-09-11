using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FR8Runtime.CodeUtility
{
    public static class UIUtility
    {
        public static void MakeButtonList(Button template, params (string, UnityAction)[] buttonDefs)
        {
            var buttons = new List<Button>();
            buttons.Add(template);
            for (var i = 1; i < buttonDefs.Length; i++)
            {
                var instance = UnityEngine.Object.Instantiate(template, template.transform.parent);
                buttons.Add(instance);
            }

            for (var i = 0; i < buttonDefs.Length; i++)
            {
                var button = buttons[i];
                var def = buttonDefs[i];

                button.name = def.Item1;

                var text = button.GetComponentInChildren<TMP_Text>();
                text.text = def.Item1;

                button.onClick.AddListener(def.Item2);
            }
        }
    }
}