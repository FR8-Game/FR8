using System;
using System.Collections.Generic;
using FR8Runtime.Save;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace FR8Runtime.UI
{
    [RequireComponent(typeof(UIDocument))]
    [SelectionBase, DisallowMultipleComponent]
    public class SettingsMenu : Menu
    {
        public VisualTreeAsset sliderCombo;

        private VisualElement content;

        private List<Resolution> resolutions;
        private List<FullScreenMode> displayModes;

        public override void Setup()
        {
            content = root.Q<ScrollView>("content").contentContainer;

            displayModes = new List<FullScreenMode>
            {
                FullScreenMode.ExclusiveFullScreen,
                FullScreenMode.FullScreenWindow,
                FullScreenMode.MaximizedWindow,
                FullScreenMode.Windowed,
            };
            resolutions = new List<Resolution>();
            resolutions.Add(new Resolution()
            {
                width = Screen.width,
                height = Screen.height,
                refreshRate = Screen.currentResolution.refreshRate,
            });
            resolutions.AddRange(Screen.resolutions);

            var settings = SaveManager.PersistantSave.GetOrLoad();

            MakeCategory
            (
                "Graphics",
                Dropdown("Resolution", 0, i =>
                    {
                        var res = resolutions[i];
                        settings.xResolution = res.width;
                        settings.yResolution = res.height;
                        settings.refreshRate = res.refreshRate;
                    },
                    GetChoices(resolutions, res => $"{res.width} x {res.height} : {res.refreshRate}hz")),
                Dropdown("Quality", 0, i => settings.quality = i, "Low", "Medium", "High"),
                Dropdown("Display", settings.displayMode, i => settings.displayMode = i, GetChoices(displayModes, DisplayModeString))
            );
            MakeCategory
            (
                "Audio",
                SliderCombo("Master", 0.0f, 100.0f, settings.masterVolume * 100.0f, v => settings.masterVolume = v / 100.0f),
                SliderCombo("Sound FX", 0.0f, 100.0f, settings.sfxVolume * 100.0f, v => settings.sfxVolume = v / 100.0f),
                SliderCombo("Music", 0.0f, 100.0f, settings.musicVolume * 100.0f, v => settings.musicVolume = v / 100.0f)
            );
            MakeCategory
            (
                "Accessibility",
                SliderCombo("Field of View", 60.0f, 110.0f, settings.fieldOfView, v => settings.fieldOfView = v),
                SliderCombo("White Noise", 0.0f, 100.0f, settings.whiteNoiseVolume * 100.0f, v => settings.whiteNoiseVolume = v / 100.0f),
                SliderCombo("Mouse Sensitivity", 10.0f, 100.0f, settings.mouseSensitivity * 100.0f, v => settings.mouseSensitivity = v / 100.0f),
                SliderCombo("Gamepad Sensitivity [X]", 10.0f, 100.0f, settings.gamepadSensitivityX * 100.0f, v => settings.gamepadSensitivityX = v / 100.0f),
                SliderCombo("Gamepad Sensitivity [Y]", 10.0f, 100.0f, settings.gamepadSensitivityY * 100.0f, v => settings.gamepadSensitivityY = v / 100.0f)
            );

            var footer = root.Q("footer");

            footer.Add(new Button(ApplySettings)
            {
                text = "APPLY"
            });
            footer.Add(new Button(RevertSettings)
            {
                text = "REVERT"
            });
            footer.Add(new Button(Pop)
            {
                text = "RETURN"
            });
        }

        private void ApplySettings()
        {
            SaveManager.PersistantSave.Save();

            var settings = SaveManager.PersistantSave.GetOrLoad();
            Screen.SetResolution(settings.xResolution, settings.yResolution, (FullScreenMode)settings.displayMode, settings.refreshRate);
            Graphics.activeTier = (GraphicsTier)settings.quality;
        }

        private void RevertSettings()
        {
            SaveManager.PersistantSave.Load();  
            Pop();
            Push(this);
        }

        private void MakeCategory(string title, params VisualElement[] elements)
        {
            var root = new VisualElement();

            var heading = new Label();
            heading.text = title.ToUpper();
            heading.AddToClassList("h4");
            root.Add(heading);

            foreach (var e in elements)
            {
                root.Add(e);
            }

            content.Add(root);
        }

        private VisualElement Dropdown(string label, int initialValue, Action<int> set, params string[] choices)
        {
            var element = new DropdownField();
            element.label = label;
            element.choices.AddRange(choices);
            element.index = initialValue;
            element.RegisterValueChangedCallback(_ => set(element.index));

            return element;
        }

        private VisualElement SliderCombo(string label, float min, float max, float initialValue, Action<float> set)
        {
            var element = sliderCombo.Instantiate();

            var slider = element.Q<Slider>();
            var textField = element.Q<TextField>();

            slider.label = label;
            slider.lowValue = min;
            slider.highValue = max;
            slider.value = initialValue;
            slider.RegisterValueChangedCallback(v =>
            {
                textField.SetValueWithoutNotify(v.newValue.ToString());
                set(v.newValue);
            });

            textField.label = "";

            textField.value = initialValue.ToString();
            textField.RegisterValueChangedCallback(str =>
            {
                if (!float.TryParse(str.newValue, out var v)) return;

                slider.SetValueWithoutNotify(v);
                set(v);
            });

            return element;
        }

        private static string[] GetChoices<T>(List<T> src, Func<T, string> toString)
        {
            var dst = new string[src.Count];
            for (var i = 0; i < src.Count; i++)
            {
                var e = src[i];
                dst[i] = toString(e);
            }

            return dst;
        }

        private static string DisplayModeString(FullScreenMode mode) => mode switch
        {
            FullScreenMode.ExclusiveFullScreen => "Fullscreen",
            FullScreenMode.FullScreenWindow => "Borderless Fullscreen",
            FullScreenMode.Windowed => "Windowed",
            FullScreenMode.MaximizedWindow => "Maximized Windowed",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}