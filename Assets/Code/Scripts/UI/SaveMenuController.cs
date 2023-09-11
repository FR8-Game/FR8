using System;
using FR8Runtime.Save;
using UnityEngine.UIElements;

namespace FR8Runtime.UI
{
    public static class SaveMenuController
    {
        public static void ReloadList(PlayerMenuController caller, VisualElement root, VisualTreeAsset saveGroupTemplate)
        {
            LoadMenuController.ReloadList(caller, root, saveGroupTemplate);

            var textField = root.Q<TextField>("text-field");
            var save = root.Q<Button>("save");

            textField.value = $"{SaveManager.saveName}.{DateTime.UtcNow:yyyy-mm-dd-hh-mm-ss}";

            save.clickable.clicked += TrySave(caller, root, saveGroupTemplate, textField);
        }

        private static Action TrySave(PlayerMenuController caller, VisualElement root, VisualTreeAsset saveGroupTemplate, TextField textField) => () =>
        {
            var saveName = textField.text;
            
            SaveManager.saveName = saveName;
            SaveManager.SaveGame();

            ReloadList(caller, root, saveGroupTemplate);
        };
    }
}