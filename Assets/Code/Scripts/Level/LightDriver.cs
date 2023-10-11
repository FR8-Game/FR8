using UnityEngine;
using UnityEngine.Rendering;

namespace FR8Runtime.Level
{
    [SelectionBase, DisallowMultipleComponent]
    public class LightDriver : MonoBehaviour
    {
        public bool state;
        [Range(500.0f, 15000.0f)] public float temperature = 5600.0f;
        public Color color;
        public float lightIntensity = 20.0f;
        public float materialIntensity = 2.0f;
        public float warmUpTime = 0.5f;
        public float cooldownTime = 1.0f;
        public AnimationCurve smoothingCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        [Space]
        [SerializeField] private new Renderer renderer;
        [SerializeField] private int materialIndex = 1;
        [SerializeField] private string materialEmissionPropertyName = "_EmissionColor";

        private float percent;
        private Material material;
        private new Light light;

        public Color EvaluatedColor => Mathf.CorrelatedColorTemperatureToRGB(temperature);

        protected virtual void Awake()
        {
            if (renderer && renderer.sharedMaterials.Length > materialIndex)
            {
                var materials = renderer.sharedMaterials;
                material = Instantiate(materials[materialIndex]);
                materials[materialIndex] = material;
                renderer.sharedMaterials = materials;
                
                material.hideFlags = HideFlags.HideAndDontSave;
            }

            light = GetLight();
        }

        protected virtual void FixedUpdate()
        {
            percent = Mathf.Clamp01(percent + Time.deltaTime / (state ? warmUpTime : -cooldownTime));
            var smoothedPercent = smoothingCurve.Evaluate(percent);

            if (material)
            {
                material.SetKeyword(new LocalKeyword(material.shader, "_EMISSION"), true);
                material.SetColor(materialEmissionPropertyName, EvaluatedColor * smoothedPercent * materialIntensity);
            }

            if (light)
            {
                light.color = EvaluatedColor;
                light.intensity = smoothedPercent * lightIntensity;
            }
        }

        private void Reset()
        {
            renderer = GetComponentInChildren<Renderer>();
        }

        private void OnValidate()
        {
            color = EvaluatedColor;
            var light = GetLight();
            
            if (!light) return;
            light.color = EvaluatedColor;
            light.intensity = lightIntensity;
        }

        private Light GetLight() => GetComponentInChildren<Light>();
    }
}