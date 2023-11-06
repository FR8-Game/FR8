using FR8Runtime.Editor;
using FR8Runtime.Train.Electrics;
using TMPro;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class RadialDisplay : MonoBehaviour, IElectricDevice
    {
        [SerializeField] private string fuseGroup = "Dash";

        [Space]
        [SerializeField] [ReadOnly] private TMP_Text valueText;
        [SerializeField] [ReadOnly] private Transform needle;
        [SerializeField] [ReadOnly] private TMP_Text labelText;
        [SerializeField] private float needleRotationMin = 80.0f;
        [SerializeField] private float needleRotationMax = -80.0f;
        [SerializeField] private float powerDrawWatts;

        [SerializeField] private float valueMin;
        [SerializeField] private float valueMax = 100.0f;
        [SerializeField] private bool clampDisplay = true;
        [SerializeField] private string valueFormat = "{0}";

        [Space(36)]
        [SerializeField] private float testValue;

        private bool powered;

        private DriverNetwork driverNetwork;

        public string DisplayValue => FormatDisplayValue(Value);
        public float Value { get; private set; }
        public string Key => name;

        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();

            labelText = transform.Find("LabelText")?.GetComponent<TMP_Text>();
            valueText = transform.Find("ValueText")?.GetComponent<TMP_Text>();

            needle = transform.Find("Gauge/Gauge_Parent/GuageNeedle_Parent");
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
            Value = driverNetwork.GetValue(Key);

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
        public string FormatDisplayValue(float value) => string.Format(valueFormat, clampDisplay ? Mathf.Clamp(value, valueMin, valueMax) : value);
    }
}