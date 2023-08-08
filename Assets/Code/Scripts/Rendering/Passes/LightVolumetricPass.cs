using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering.Passes
{
    public class LightVolumetricPass : ScriptableRenderPass
    {
        private int planeCount;
        private float farPlane;
        private float density;
        
        private static readonly int VolumetricsPercent = Shader.PropertyToID("_Volumetrics_Percent");
        private static readonly int VolumetricsResolution = Shader.PropertyToID("_Volumetrics_Resolution");
        private static readonly int VolumetricsDensity = Shader.PropertyToID("_Volumetrics_Density");

        public LightVolumetricPass(int planeCount, float farPlane, float density)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            
            this.planeCount = planeCount;
            this.farPlane = farPlane;
            this.density = density;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            var camera = renderingData.cameraData.camera;
            
            Shader.SetGlobalInt(VolumetricsResolution, planeCount);
            Shader.SetGlobalFloat(VolumetricsPercent, farPlane / (camera.farClipPlane - camera.nearClipPlane));
            Shader.SetGlobalFloat(VolumetricsDensity, density);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var shader = Shader.Find("Hidden/Volumetrics");
            if (!shader) return;

            var cmd = CommandBufferPool.Get("Volumetrics");

            var mat = new Material(shader);
            cmd.DrawProcedural(Matrix4x4.identity, mat, 0, MeshTopology.Triangles, planeCount * 3);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}