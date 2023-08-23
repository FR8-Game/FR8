using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8Runtime.Rendering.Passes
{
    public class PlacementPass : ScriptableRenderPass
    {
        private Material drawMaterial;
        private Material blitMaterial;

        private static readonly int TargetPass = Shader.PropertyToID("_PlacementPass");
        
        public static List<Renderer> renderers;

        public PlacementPass(Material drawMaterial, Material blitMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            renderers = new List<Renderer>();
            
            this.drawMaterial = drawMaterial;
            this.blitMaterial = blitMaterial;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            cmd.GetTemporaryRT(TargetPass, renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!drawMaterial) return;
            if (!blitMaterial) return;
            
            var cmd = CommandBufferPool.Get("Placement Pass");

            cmd.SetRenderTarget(TargetPass);
            
            foreach (var renderer in renderers)
            {
                cmd.DrawRenderer(renderer, drawMaterial, 0, 0);
            }
            
            cmd.Blit(TargetPass, renderingData.cameraData.renderer.cameraColorTarget, blitMaterial);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            cmd.ReleaseTemporaryRT(TargetPass);
            renderers.Clear();
        }
    }
}