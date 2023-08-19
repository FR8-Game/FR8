using FR8.Rendering.Passes;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering
{
    public sealed class CustomRenderFeatures : ScriptableRendererFeature
    {
        [SerializeField] private FogPass.Settings fogSettings;
        [SerializeField] private SelectionOutlinePass.Settings outlineSettings;
        [SerializeField] private LightVolumetricPass.Settings volumetricsSettings;
        [SerializeField] private MapMarkerPass.Settings mapMarkerSettings;
        [SerializeField] private CloudPass.Settings cloudSettings;

        private FogPass fogPass;
        private SelectionOutlinePass outlinePass;
        private LightVolumetricPass volumetricPass;
        private MapMarkerPass mapMarkersPass;
        private CloudPass cloudPass;

        public override void Create()
        {
            fogPass = new FogPass(fogSettings);
            outlinePass = new SelectionOutlinePass(outlineSettings);
            volumetricPass = new LightVolumetricPass(volumetricsSettings);
            mapMarkersPass = new MapMarkerPass(mapMarkerSettings);
            cloudPass = new CloudPass(cloudSettings);
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
