using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.UI.CustomControls
{
    public class Compass : VisualElement
    {
        private float faceAngle;
        private float cropAngle = 90.0f;
        private float tickSpacing = 22.5f;

        private Label[] labels;
        private VisualElement[] ticks;

        public float FaceAngle
        {
            get => faceAngle;
            set
            {
                faceAngle = value;
                Update();
                MarkDirtyRepaint();
            }
        }

        public float CropAngle
        {
            get => cropAngle;
            set
            {
                cropAngle = value;
                Update();
                MarkDirtyRepaint();
            }
        }

        public float TickSpacing
        {
            get => tickSpacing;
            set
            {
                tickSpacing = value;
                Update();
                MarkDirtyRepaint();
            }
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlFloatAttributeDescription faceAngle = new() { name = "FaceAngle" };
            private UxmlFloatAttributeDescription cropAngle = new() { name = "CropAngle", defaultValue = 90.0f };
            private UxmlFloatAttributeDescription tickSpacing = new() { name = "TickSpacing", defaultValue = 22.5f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                ((Compass)ve).FaceAngle = faceAngle.GetValueFromBag(bag, cc);
                ((Compass)ve).CropAngle = cropAngle.GetValueFromBag(bag, cc);
                ((Compass)ve).TickSpacing = tickSpacing.GetValueFromBag(bag, cc);
            }
        }

        public new class UxmlFactory : UxmlFactory<Compass, UxmlTraits> { }

        private static readonly string[] LabelTexts =
        {
            "N", "E", "S", "W",
        };

        public Compass()
        {
            Clear();

            labels = new Label[4];
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i] = new Label();
                labels[i].text = LabelTexts[i];
                Add(labels[i]);
            }

            style.flexDirection = FlexDirection.Row;

            RegenTicks();
            Update();
        }

        private void RegenTicks()
        {
            if (ticks != null)
            {
                foreach (var t in ticks)
                {
                    Remove(t);
                }
            }

            if (tickSpacing < 1.0f) tickSpacing = 1.0f;
            ticks = new VisualElement[Mathf.FloorToInt((360.0f / tickSpacing) * (cropAngle / 360.0f))];

            for (var i = 0; i < ticks.Length; i++)
            {
                ticks[i] = new VisualElement();
                Add(ticks[i]);
            }
        }

        private void Update()
        {
            if (ticks == null) RegenTicks();
            if (ticks.Length != tickSpacing) RegenTicks();

            const int tickWidth = 2;
            const int labelWidth = 30;

            var cropAngle = this.cropAngle * Mathf.Deg2Rad;
            var faceAngle = this.faceAngle * Mathf.Deg2Rad;

            for (var i = 0; i < ticks.Length; i++)
            {
                var tick = ticks[i];

                var p = i / (ticks.Length - 1.0f);
                setPositionOffAngle(tick, (2.0f * p - 1.0f) * cropAngle - faceAngle, tickWidth, 2.0f, out var scale);
                tick.style.scale = new Scale(new Vector3(1.0f, scale, 1.0f));

                tick.style.backgroundImage = Texture2D.whiteTexture;
                tick.style.unityBackgroundImageTintColor = Color.white;
                tick.style.scale = new Scale(new Vector3(1.0f, tick.style.scale.value.value.y, 0.0f));

                var shrink = i % 2 == 0;

                tick.style.top = Length.Percent(shrink ? 12.5f : 0.0f);
                tick.style.bottom = Length.Percent(shrink ? 62.5f : 50.0f);
            }
            
            var facing = Mathf.RoundToInt(this.faceAngle / 90.0f) % 4;

            for (var i = 0; i < labels.Length; i++)
            {
                var label = labels[i];

                var percent = i / (float)labels.Length;
                var angle = percent * Mathf.PI * 2.0f;
                setPositionOffAngle(label, angle - faceAngle, labelWidth, 2.0f, out var scale);
                label.style.scale = new Scale(Vector3.one * scale);
                
                label.visible = i == facing;

                label.style.unityTextAlign = TextAnchor.UpperCenter;
                label.style.top = Length.Percent(50.0f);
                label.style.bottom = 0.0f;
            }

            float sqr(float x) => x * x;

            void setPositionOffAngle(VisualElement target, float angle, float elementWidth, float sf, out float scale)
            {
                angle /= cropAngle;
                angle = angle * 0.5f + 0.5f;
                angle = (angle % 1.0f + 1.0f) % 1.0f;
                angle = angle * 2.0f - 1.0f;
                angle *= cropAngle;

                var p = Mathf.Sin(angle) * 0.5f + 0.5f;

                target.style.position = Position.Absolute;
                target.style.left = Length.Percent(p * 100.0f);
                target.style.width = elementWidth;
                target.style.translate = new Translate(elementWidth / -2.0f, 0.0f, 0.0f);

                scale = Mathf.Exp(-sqr(sf * (p * 2.0f - 1.0f)));
            }
        }
    }
}