using UnityEngine;

namespace FR8.Level
{
    [SelectionBase, DisallowMultipleComponent]
    public class LightDriver : MonoBehaviour
    {
        public bool state;
        public Color color = Mathf.CorrelatedColorTemperatureToRGB(1500.0f);
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

        protected virtual void Awake()
        {
            if (renderer && renderer.sharedMaterials.Length > materialIndex)
            {
                material = Instantiate(renderer.sharedMaterials[materialIndex]);
                renderer.sharedMaterials[materialIndex] = material;
                material.hideFlags = HideFlags.HideAndDontSave;
            }

            light = GetComponentInChildren<Light>();
        }

        protected virtual void FixedUpdate()
        {
            percent = Mathf.Clamp01(percent + (state ? warmUpTime : cooldownTime) * Time.deltaTime);
            var smoothedPercent = smoothingCurve.Evaluate(percent);

            if (material)
            {
                material.SetColor(materialEmissionPropertyName, color * smoothedPercent * materialIndex);
            }

            if (light)
            {
                light.color = color;
                light.intensity = smoothedPercent * lightIntensity;
            }
        }

        private void Reset()
        {
            renderer = GetComponentInChildren<Renderer>();
        }
    }
}