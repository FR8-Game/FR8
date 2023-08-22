using FR8Runtime.Rendering.Passes;
using UnityEngine.Rendering.Universal;

namespace FR8Runtime.Rendering
{
    public sealed class CustomRenderFeatures : ScriptableRendererFeature
    {
        private FogPass fogPass;
        private SelectionOutlinePass outlinePass;
        private LightVolumetricPass volumetricPass;
        private MapMarkerPass mapMarkersPass;
        private CloudPass cloudPass;

        public override void Create()
        {
            fogPass = new FogPass();
            outlinePass = new SelectionOutlinePass();
            volumetricPass = new LightVolumetricPass();
            mapMarkersPass = new MapMarkerPass();
            cloudPass = new CloudPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            fogPass.Enqueue(renderer);
            outlinePass.Enqueue(renderer);
            volumetricPass.Enqueue(renderer);
            mapMarkersPass.Enqueue(renderer);
            cloudPass.Enqueue(renderer);
        }
    }
}
