using FR8.Interactions.Drivables;
using FR8.Train.Electrics;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class RadialDisplay : MonoBehaviour, IDrivable, IElectricDevice
    {
        [SerializeField] private string key;

        [Space]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private DriverDisplayText displayText;
        [SerializeField] private DriverDisplayUI displayUI;
        [SerializeField] private float powerDrawWatts;

        public string DisplayValue => Mathf.RoundToInt(Value * 100.0f).ToString();
        public float Value { get; private set; }
        public string Key => key;

        public void OnValueChanged(float newValue)
        {
            Value = newValue;
            
            displayText.SetValue(newValue);
            displayUI.SetValue(newValue);
        }

        private void Awake()
        {
            displayText.Awake();
        }

        private void OnValidate()
        {
            displayUI.OnValidate();
        }

        public void SetConnected(bool connected)
        {
            group.alpha = connected ? 1.0f : 0.0f;
            group.interactable = connected;
            group.blocksRaycasts = connected;
        }

        public float CalculatePowerDraw() => powerDrawWatts / 1000.0f;
    }
}