using System.Collections;
using FR8Runtime.Interactions.Drivables;
using FR8Runtime.Train.Electrics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class RadialDisplay : MonoBehaviour, IElectricDevice
    {
        [SerializeField] private string key;
        [SerializeField] private string fuseGroup = "Dash";

        [Space]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private DriverDisplayText displayText;
        [SerializeField] private DriverDisplayUI displayUI;
        [SerializeField] private float powerDrawWatts;

        [Space]
        [SerializeField] private float fadeTime = 0.1f;

        [SerializeField] private float powerUpTime = 4.0f;
        [SerializeField] private float powerUpVariance = 0.4f;

        [Space]
        [SerializeField] private DisplayMode displayMode = DisplayMode.Raw;
        [SerializeField] private float testValue;

        private bool powered;
        private bool warmingUp;

        private DriverNetwork driverNetwork;

        public string DisplayValue => Mathf.RoundToInt(Value * 100.0f).ToString();
        public float Value { get; private set; }

        private void Awake()
        {
            driverNetwork = GetComponentInParent<DriverNetwork>();
            
            displayText.Awake(() => displayMode);
            displayUI.Awake(() => displayMode);
        }

        private void OnValidate()
        {
            Awake();
            
            displayUI.OnValidate(testValue);
            displayText.OnValidate(testValue);
        }

        private void FixedUpdate()
        {
            UpdatePowerState();
            Value = driverNetwork.GetValue(key);
            displayText.SetValue(Value);
            displayUI.SetValue(Value);

            if (group) group.alpha += ((powered ? 1.0f : 0.0f) - group.alpha) * 2.0f * Time.deltaTime / fadeTime;
            
            displayText.FixedUpdate();
        }
        
        public void UpdatePowerState()
        {
            var powered = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f
                && driverNetwork.GetValue(fuseGroup) > 0.5f;

            if (this.powered == powered) return;
            
            group.interactable = powered;
            group.blocksRaycasts = powered;
            this.powered = powered;

            if (powered)
            {
                StartCoroutine(PowerUpRoutine());
            }
            else
            {
                StopAllCoroutines();
                warmingUp = false;
            }
        }

        private IEnumerator PowerUpRoutine()
        {
            if (warmingUp) yield break;
            warmingUp = true;
            
            var duration = powerUpTime + Random.Range(-powerUpVariance, powerUpVariance);
            var t = 0.0f;
            while (t < duration)
            {
                displayText.SetText(new string('.', Mathf.FloorToInt(t % 1.0f * 3.0f + 1.0f)));

                if (t < 2.0f)
                    displayUI.SetValueNormalized(t / 2.0f);
                else
                    displayUI.SetValueNormalized(t * 2.0f % 1.0f > 0.5f ? 1.0f : 0.0f);

                t += Time.deltaTime;
                yield return null;
            }
            
            displayText.SetValue(Value);
            displayUI.SetValue(Value);

            warmingUp = false;
        }

        public float CalculatePowerDraw() => powerDrawWatts / 1000.0f;
        
        public enum DisplayMode
        {
            Raw,
            Percentage
        }
    }
}