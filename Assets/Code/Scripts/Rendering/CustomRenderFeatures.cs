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
        [SerializeField] private bool renderFogOverSkybox;
        
        private FogPass fogPass;
        private SelectionOutlinePass outlinePass;
        private LightVolumetricsPass volumetricsPass;

        public override void Create()
        {
            fogPass = new FogPass(renderFogOverSkybox);
            outlinePass = new SelectionOutlinePass();
            volumetricsPass = new LightVolumetricsPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderFog) renderer.EnqueuePass(fogPass);
            if (renderOutline) renderer.EnqueuePass(outlinePass);
            if (volumetrics) renderer.EnqueuePass(volumetricsPass);
        }
    }
}
