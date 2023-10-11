using FR8Runtime.Rendering.Passes;
using UnityEngine.Rendering.Universal;

namespace FR8Runtime.Rendering
{
    public sealed class CustomRenderFeatures : ScriptableRendererFeature
    {
        public bool renderFog = true;
        public bool renderOutline = true;
        public bool renderMapMarker = true;

        private FogPass fogPass;
        private SelectionOutlinePass outlinePass;
        private MapMarkerPass mapMarkersPass;

        public override void Create()
        {
            if (renderFog) fogPass = new FogPass();
            if (renderOutline) outlinePass = new SelectionOutlinePass();
            if (renderMapMarker) mapMarkersPass = new MapMarkerPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderFog) fogPass.Enqueue(renderer);
            if (renderOutline) outlinePass.Enqueue(renderer);
            if (renderMapMarker) mapMarkersPass.Enqueue(renderer);
        }
    }
}
