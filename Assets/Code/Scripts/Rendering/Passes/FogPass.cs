using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Runtime.Rendering.Passes
{
    public sealed class FogPass : CustomRenderPass<FogSettings>
    {
        private Shader fogShader;
        private Material fogMaterial;

        private static readonly int FogColor = Shader.PropertyToID("_FogColor");
        private static readonly int FogAttenuation = Shader.PropertyToID("_FogAttenuation");

        public override bool Enabled => Settings.active;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            renderPassEvent = Settings.renderOverSkybox.value ? RenderPassEvent.AfterRenderingSkybox : RenderPassEvent.BeforeRenderingSkybox;

            fogShader = Shader.Find("Hidden/Fog");
            if (fogShader) fogMaterial = CoreUtils.CreateEngineMaterial(fogShader);

            var fogColor = Settings.color;
            var fogAttenuation = new Vector4
            {
                x = Mathf.Pow(Settings.density.value, 10.0f),
                y = Settings.heightFalloffLower.value,
                z = Settings.heightFalloffUpper.value
            };

            cmd.SetGlobalColor(FogColor, fogColor.value);
            cmd.SetGlobalColor(FogAttenuation, fogAttenuation);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera && !Settings.showFogInSceneView.value) return;
            ExecuteWithCommandBuffer(context, cmd => cmd.DrawProcedural(Matrix4x4.identity, fogMaterial, 0, MeshTopology.Triangles, 3));
        }
    }
}