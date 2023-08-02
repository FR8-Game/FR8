using FR8.Interactions.Drivables;
using UnityEngine;

namespace FR8.Train.Electrics
{
    public class Light : MonoBehaviour, IElectricDevice, IDrivable
    {
        [SerializeField] private bool state;
        [SerializeField] private string key;
        [SerializeField] private Renderer filamentRenderer;
        [SerializeField] private Gradient color;
        [SerializeField] private float lightBrightness = 1.5f;
        [SerializeField] private float materialBrightness = 50.0f;
        [SerializeField] private float warmupTime = 3.0f;
        [SerializeField] private float cooldownTime = 15.0f;
        [SerializeField] private float powerDrawWatts = 40.0f;
        [SerializeField] private float remapExponent = 2.0f;
        
        private new UnityEngine.Light light;
        private float percent;
        private bool connected;
        private MaterialPropertyBlock materialPropertyBlock;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private float Luminosity => Mathf.Pow(percent, remapExponent);
        public bool On => connected && state;
        public string Key => key;

        private void Awake()
        {
            light = GetComponentInChildren<UnityEngine.Light>();
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetConnected(bool connected)
        {
            this.connected = connected;
        }

        private void FixedUpdate()
        {
            percent += ((On ? 1.0f : 0.0f) - percent) * Time.deltaTime * 2.0f / (On ? warmupTime : cooldownTime);
            UpdateLight();
            UpdateMaterial();
        }

        private void UpdateLight()
        {
            if (!light) return;

            light.intensity = lightBrightness * Luminosity;
            light.color = color.Evaluate(Luminosity);
        }

        private void UpdateMaterial()
        {
            if (!filamentRenderer) return;

            materialPropertyBlock.SetColor(EmissionColor, color.Evaluate(Luminosity) * materialBrightness * Luminosity);
            filamentRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        public float CalculatePowerDraw() => On ? powerDrawWatts / 1000.0f : 0.0f;
        
        public void OnValueChanged(float newValue)
        {
            state = newValue > 0.5f;
        }
    }
}
