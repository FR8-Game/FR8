using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8Runtime.Rendering.Passes
{
    [Serializable]
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

    [VolumeComponentMenu("Custom/Fog")]
    public class FogSettings : VolumeComponent
    {
        public ColorParameter color = new(new Color(1.0f, 1.0f, 1.0f, 0.1f));
        public ClampedFloatParameter density = new(0.5f, 0.0f, 1.0f);
        public FloatParameter heightFalloffLower = new(5.0f);
        public FloatParameter heightFalloffUpper = new(30.0f);
        public BoolParameter renderOverSkybox = new(false);
        public BoolParameter showFogInSceneView = new(false);
    }
}