using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.UI.CustomControls
{
    public sealed class LoadBar : Label
    {
        private int characterCount = 15;
        private float percent;

        private string append;
        private string prepend;

        public int CharacterCount
        {
            get => characterCount;
            set
            {
                characterCount = value;
                Update();
                MarkDirtyRepaint();
            }
        }

        public float Percent
        {
            get => percent;
            set
            {
                percent = value;
                Update();
                MarkDirtyRepaint();
            }
        }

        public string Append
        {
            get => append;
            set
            {
                append = value;
                Update();
                MarkDirtyRepaint();
            }
        }

        public string Prepend
        {
            get => prepend;
            set
            {
                prepend = value;
                Update();
                MarkDirtyRepaint();
            }
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlIntAttributeDescription characterCount = new() { name = "CharacterCount", defaultValue = 15 };
            private UxmlStringAttributeDescription append = new() { name = "Append" };
            private UxmlStringAttributeDescription prepend = new() { name = "Prepend" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                ((LoadBar)ve).CharacterCount = characterCount.GetValueFromBag(bag, cc);
                ((LoadBar)ve).Append = append.GetValueFromBag(bag, cc);
                ((LoadBar)ve).Prepend = prepend.GetValueFromBag(bag, cc);
            }
        }

        public new class UxmlFactory : UxmlFactory<LoadBar, UxmlTraits> { }

        public void Update()
        {
            var c0 = Mathf.Clamp(Mathf.FloorToInt(percent * characterCount), 0, characterCount);
            var c1 = characterCount - c0;

            text = $"{prepend}[{new string('=', c0)}{new string('-', c1)}] {StringUtility.Percent(percent)}{append}";
        }
    }
}