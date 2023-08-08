using FR8.Rendering.Passes;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering
{
    public sealed class CustomRenderFeatures : ScriptableRendererFeature
    {
        [SerializeField] private bool renderFog = true;
        [SerializeField] private bool renderOutline = true;
        [SerializeField] private bool volumetrics = true;

        [Space]
        [SerializeField] private int volumetricsResolution = 1000;
        [SerializeField] private float volumetricsFarPlane = 100.0f;
        [SerializeField] private float volumetricsDensity = 0.2f;
        
        private FogPass fogPass;
        private SelectionOutlinePass outlinePass;
        private LightVolumetricPass volumetricPass;

        public override void Create()
        {
            fogPass = new FogPass();
            outlinePass = new SelectionOutlinePass();
            volumetricPass = new LightVolumetricPass(volumetricsResolution, volumetricsFarPlane, volumetricsDensity);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderFog) renderer.EnqueuePass(fogPass);
            if (renderOutline) renderer.EnqueuePass(outlinePass);
            if (volumetrics) renderer.EnqueuePass(volumetricPass);
        }
    }
}
