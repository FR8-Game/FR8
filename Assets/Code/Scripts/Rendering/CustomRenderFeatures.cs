using UnityEngine.Rendering.Universal;

namespace FR8.Rendering
{
    public sealed class CustomRenderFeatures : ScriptableRendererFeature
    {
        private FogPass fogPass;
        private SelectionOutlinePass outlinePass;

        public override void Create()
        {
            fogPass = new FogPass();
            outlinePass = new SelectionOutlinePass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(fogPass);
            renderer.EnqueuePass(outlinePass);
        }
    }
}
