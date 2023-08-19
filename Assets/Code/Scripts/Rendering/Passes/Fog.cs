using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering.Passes
{
    [Serializable]
    public sealed class FogPass : CustomRenderPass
    {
        public Settings settings;

        private Shader fogShader;
        private Material fogMaterial;

        private static readonly int FogColor = Shader.PropertyToID("_FogColor");
        private static readonly int FogAttenuation = Shader.PropertyToID("_FogAttenuation");

        public override bool Enabled => settings.enabled && fogMaterial;

        public FogPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderOverSkybox ? RenderPassEvent.AfterRenderingSkybox : RenderPassEvent.BeforeRenderingSkybox;

            fogShader = Shader.Find("Hidden/Fog");
            if (fogShader) fogMaterial = CoreUtils.CreateEngineMaterial(fogShader);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var fogColor = settings.color;
            var fogAttenuation = new Vector4
            {
                x = Mathf.Pow(settings.density, 10.0f),
                y = settings.heightFalloffLower,
                z = settings.heightFalloffUpper,
            };

            cmd.SetGlobalColor(FogColor, fogColor);
            cmd.SetGlobalColor(FogAttenuation, fogAttenuation);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera && !settings.showFogInSceneView) return;
            ExecuteWithCommandBuffer(context, cmd => cmd.DrawProcedural(Matrix4x4.identity, fogMaterial, 0, MeshTopology.Triangles, 3));
        }

        [Serializable]
        public class Settings
        {
            public bool enabled = true;
            public Color color = new(1.0f, 1.0f, 1.0f, 0.1f);
            [Range(0.0f, 1.0f)] public float density = 0.5f;
            public float heightFalloffLower = 5.0f;
            public float heightFalloffUpper = 30.0f;
            public bool renderOverSkybox;
            public bool showFogInSceneView;
        }
    }
}