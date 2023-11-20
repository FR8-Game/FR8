using FR8.Runtime.Editor;
using FR8.Runtime.Train.Electrics;
using TMPro;
using UnityEngine;

namespace FR8.Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class RadialDisplay : MonoBehaviour, IElectricDevice
    {
        private const float ValueSmoothing = 0.1f;
        
        [SerializeField] private string fuseGroup = "Dash";

        [Space]
        [SerializeField] [ReadOnly] private TMP_Text valueText;
        [SerializeField] [ReadOnly] private Transform needle;
        [SerializeField] [ReadOnly] private TMP_Text labelText;
        [SerializeField] private float needleRotationMin = 80.0f;
        [SerializeField] private float needleRotationMax = -80.0f;
        [SerializeField] private float powerDrawWatts;

        [Space(36)]
        [SerializeField] private float testValue;
        
        private float smoothedValue;
        private float valueMin;
        private float valueMax = 1.0f;
        private bool clampDisplay = true;
        private float displayValueScale = 1.0f;
        private string valueFormat = "{0:N0}";

        private bool powered;

        private DriverNetwork driverNetwork;

        public string DisplayValue => FormatDisplayValue(Value);
        public float Value => smoothedValue;
        public string Key => name;

        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();

            labelText = transform.Find("LabelText")?.GetComponent<TMP_Text>();
            valueText = transform.Find("ValueText")?.GetComponent<TMP_Text>();

            needle = transform.Find("Gauge/Gauge_Parent/GuageNeedle_Parent");

            FormatDisplay();
        }

        private void FormatDisplay()
        {
            switch (Key.ToLower().Replace(" ", ""))
            {
                case "rpm":
                    valueMax = 10000.0f;
                    break;
                case "speed":
                    valueMax = 200.0f;
                    break;
                case "load":
                case "fuel":
                case "powerdraw":
                    displayValueScale = 100.0f;
                    valueFormat = "{0:N0}%";
                    break;
                case "current":
                    valueMax = 100.0f;
                    break;
            }
        }

        private void Start()
        {
            UpdateVisualsWithValue(Random.Range(valueMin, valueMax));
        }

        private void OnValidate()
        {
            Awake();
            UpdateVisualsWithValue(testValue);
        }

        private void FixedUpdate()
        {
            UpdatePowerState();
            var realValue = driverNetwork.GetValue(Key);
            smoothedValue = Mathf.Lerp(smoothedValue, realValue, Time.deltaTime / ValueSmoothing);
            
            if (!powered) return;

            UpdateVisualsWithValue(Value);
        }

        public void UpdateVisualsWithValue(float value)
        {
            if (labelText) labelText.text = Key;
            if (valueText) valueText.text = FormatDisplayValue(value);

            if (needle)
            {
                var p = Mathf.InverseLerp(valueMin, valueMax, value);
                needle.localRotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Lerp(needleRotationMin, needleRotationMax, p));
            }
        }

        public void UpdatePowerState()
        {
            powered = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f
                          && driverNetwork.GetValue(fuseGroup) > 0.5f;

            labelText.gameObject.SetActive(powered);
            valueText.gameObject.SetActive(powered);
        }

        public float CalculatePowerDraw() => powerDrawWatts / 1000.0f;
        public string FormatDisplayValue(float value) => string.Format(valueFormat, (clampDisplay ? Mathf.Clamp(value, valueMin, valueMax) : value) * displayValueScale);
    }
}