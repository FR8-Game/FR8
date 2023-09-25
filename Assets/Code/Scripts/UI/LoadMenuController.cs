using FR8Runtime.Save;
using UnityEngine.UIElements;

namespace FR8Runtime.UI
{
    public static class LoadMenuController
    {
        public static void ReloadList(PlayerMenuController caller, VisualElement root, VisualTreeAsset saveGroupTemplate)
        {
            var saveGroups = SaveManager.GetAllSaveGroups();
            var scrollView = root.Q<ScrollView>("scroll-view");

            scrollView.Clear();

            foreach (var saveGroup in saveGroups)
            {
                var instance = saveGroupTemplate.Instantiate();
                scrollView.Add(instance);

                SetupSaveGroup(caller, saveGroup, instance);
            }
        }

        private static void SetupSaveGroup(PlayerMenuController caller, SaveManager.SaveGroup saveGroup, VisualElement root)
        {
            var title = root.Q<Label>("title");
            title.text = saveGroup.groupName;

            var container = root.Q("save-files");
            container.Clear();
            foreach (var saveName in saveGroup.saveNames)
            {
                var button = new Button();
                container.Add(button);

                button.text = saveName;
                button.clickable.clicked += caller.LoadScene(saveName);
            }
        }
    }
}