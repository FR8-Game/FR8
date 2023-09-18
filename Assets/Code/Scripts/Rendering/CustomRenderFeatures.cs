using FR8Runtime.Rendering.Passes;
using UnityEngine.Rendering.Universal;

namespace FR8Runtime.Rendering
{
    public sealed class CustomRenderFeatures : ScriptableRendererFeature
    {
        public bool renderFog = true;
        public bool renderOutline = true;
        public bool renderVolumetrics;
        public bool renderMapMarker = true;
        public bool renderClouds;

        private FogPass fogPass;
        private SelectionOutlinePass outlinePass;
        private LightVolumetricPass volumetricPass;
        private MapMarkerPass mapMarkersPass;
        private CloudPass cloudPass;

        public override void Create()
        {
            if (renderFog) fogPass = new FogPass();
            if (renderOutline) outlinePass = new SelectionOutlinePass();
            if (renderVolumetrics) volumetricPass = new LightVolumetricPass();
            if (renderMapMarker) mapMarkersPass = new MapMarkerPass();
            if (renderClouds) cloudPass = new CloudPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderFog) fogPass.Enqueue(renderer);
            if (renderOutline) outlinePass.Enqueue(renderer);
            if (renderVolumetrics) volumetricPass.Enqueue(renderer);
            if (renderMapMarker) mapMarkersPass.Enqueue(renderer);
            if (renderClouds) cloudPass.Enqueue(renderer);
        }
    }
}
