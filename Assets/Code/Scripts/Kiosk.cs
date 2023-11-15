using System.Collections.Generic;
using FR8.Runtime.CodeUtility;
using FR8.Runtime.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FR8.Runtime
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class Kiosk : MonoBehaviour
    {
        public List<Entry> entries = new();

        private int selectedEntry = -1;
        private CanvasGroup previewGroup;
        private RawImage previewImage;
        private TMP_Text previewTitle;
        private TMP_Text previewDescription;
        private Button previewButton;

        private void Awake()
        {
            SetupHierarchy();
            SelectEntry(selectedEntry)();
        }

        private void SetupHierarchy()
        {
            var buttonPrefab = transform.Find<Button>("Kiosk/Canvas/Pad/Content/Scroll View/Viewport/Content/Button");
            var buttons = new Button[entries.Count];
            buttons[0] = buttonPrefab;
            for (var i = 1; i < buttons.Length; i++)
            {
                buttons[i] = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            }

            for (var i = 0; i < buttons.Length; i++)
            {
                var b = buttons[i];
                var e = entries[i];

                b.name = e.name;
                b.transform.SetAsLastSibling();

                var text = b.transform.Find<TMP_Text>("Text");
                if (text) text.text = e.name;

                var image = b.transform.Find<RawImage>("Icon");
                if (image) image.texture = e.preview;

                b.onClick.AddListener(SelectEntry(i));
            }

            previewGroup = transform.Find<CanvasGroup>("Kiosk/Canvas/Pad/Content/Preview");
            var unpacker = new HierarchyUnpacker(previewGroup);
            
            previewImage = unpacker.Next().Get<RawImage>();
            previewTitle = unpacker.Next(2).Get<TMP_Text>();
            previewDescription = unpacker.Next().Get<TMP_Text>();
            previewButton = unpacker.Next().Get<Button>();
            
            previewButton.onClick.AddListener(Load);
        }

        public UnityAction SelectEntry(int i) => () =>
        {
            selectedEntry = i;
            var entry = selectedEntry != -1 ? entries[selectedEntry] : Entry.NullEntry;

            var visible = i != -1;
            previewGroup.alpha = visible ? 1.0f : 0.0f;
            previewButton.interactable = visible;

            previewImage.texture = entry.preview;
            previewTitle.text = entry.name;
            previewDescription.text = entry.description;
        };

        public void Load()
        {
            if (selectedEntry == -1) return;
            
            var entry = entries[selectedEntry];
            SceneUtility.LoadScene(entry.sceneIndex);
        }

        [System.Serializable]
        public class Entry
        {
            public static readonly Entry NullEntry = new ()
            {
                sceneIndex = -1,
                preview = null,
                name = "",
                description = "",
            };

            public int sceneIndex;
            public Texture2D preview;
            public string name;

            [TextArea]
            public string description;
        }
    }
}