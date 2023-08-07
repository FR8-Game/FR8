using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering.Passes
{
    public class LightVolumetricsPass : ScriptableRenderPass
    {
        private const int particles = 100000;

        private VolumetricLight[] lights;
        
        private static readonly int VolumetricColors = Shader.PropertyToID("_VolumetricColors");
        private static readonly int VolumetricPositions = Shader.PropertyToID("_VolumetricPositions");
        private static readonly int VolumetricRanges = Shader.PropertyToID("_VolumetricRanges");
        private static readonly int VolumetricDensities = Shader.PropertyToID("_VolumetricDensities");

        public LightVolumetricsPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            lights = Object.FindObjectsOfType<VolumetricLight>();

            if (lights.Length == 0) return;
            
            var colors = new Vector4[lights.Length];
            var positions = new Vector4[lights.Length];
            var range = new float[lights.Length];
            var density = new float[lights.Length];

            for (var i = 0; i < lights.Length; i++)
            {
                colors[i] = lights[i].Light.color * lights[i].Light.intensity;
                positions[i] = lights[i].transform.position;
                range[i] = lights[i].Light.range;
                density[i] = lights[i].Density;
            }
            
            Shader.SetGlobalVectorArray(VolumetricColors, colors);
            Shader.SetGlobalVectorArray(VolumetricPositions, positions);
            Shader.SetGlobalFloatArray(VolumetricRanges, range);
            Shader.SetGlobalFloatArray(VolumetricDensities, density);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (lights.Length == 0) return;
            
            var shader = Shader.Find("Hidden/Volumetrics");
            if (!shader) return;

            var cmd = CommandBufferPool.Get("Volumetrics");

            var mat = new Material(shader);
            cmd.DrawProcedural(Matrix4x4.identity, mat, 0, MeshTopology.Triangles, 6 * particles * lights.Length);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}