using FR8.Runtime.Train.Electrics;
using UnityEngine;

namespace FR8.Runtime.Level
{
    [SelectionBase, DisallowMultipleComponent]
    public class LightDriver : MonoBehaviour
    {
        public bool state;
        public LightDriverSettings settings;
        public bool emergencyLight;

        [Space]
        [SerializeField] private new Renderer renderer;
        [SerializeField] private new Light light;
        [SerializeField] private int materialIndex = 1;
        [SerializeField] private string materialEmissionPropertyName = "_Emission";

        private float percent;
        private Material material;
        private MaterialPropertyBlock propertyBlock;
        private Color materialColor;
        private Color lightColor;
        
        private float stateTime;

        protected virtual void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (renderer && renderer.sharedMaterials.Length > materialIndex)
            {
                var materials = renderer.sharedMaterials;
                material = Instantiate(materials[materialIndex]);
                materials[materialIndex] = material;
                renderer.sharedMaterials = materials;

                material.hideFlags = HideFlags.HideAndDontSave;
            }

            if (light)
            {
                lightColor = light.color;
            }

            if (material) materialColor = material.GetColor(materialEmissionPropertyName);
        }

        private void OnEnable()
        {
            if (!settings)
            {
                Debug.LogWarning("LightDriver is missing a Settings Object, please add one", this);
                settings = FindObjectOfType<LightDriverSettings>();
                if (!settings) enabled = false;
            }
        }

        protected virtual void FixedUpdate()
        {
            percent = Mathf.Clamp01(percent + Time.deltaTime / (state ? settings.warmUpTime : -settings.cooldownTime));
            var smoothedPercent = settings.smoothingCurve.Evaluate(percent);

            var attenuation = CalculateAttenuation();
            
            if (renderer && renderer.sharedMaterials.Length > materialIndex)
            {
                propertyBlock.SetColor(materialEmissionPropertyName, BlendColor(materialColor, smoothedPercent, attenuation));
                renderer.SetPropertyBlock(propertyBlock, materialIndex);
            }

            if (light)
            {
                light.color = BlendColor(lightColor, smoothedPercent, attenuation);
            }
        }

        protected virtual float CalculateAttenuation() => 1.0f;

        private Color BlendColor(Color baseColor, float p, float attenuation)
        {
            var color = Color.Lerp(emergencyLight ? settings.emergencyLightColor : Color.black, baseColor, p) * attenuation;
            color.Alpha(1.0f);
            return color;
        }

        private void Reset()
        {
            if (!renderer) renderer = GetComponentInChildren<Renderer>();
            if (!light) light = GetComponentInChildren<Light>();
            if (!settings) settings = FindObjectOfType<LightDriverSettings>();
        }
    }
}