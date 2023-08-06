using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering.Passes
{
    [VolumeComponentMenuForRenderPipeline("Custom/Fog", typeof(UniversalRenderPipeline))]
    public sealed class Fog : VolumeComponent, IPostProcessComponent
    {
        public bool IsActive() => active && density?.value > 0.0f;
        public bool IsTileCompatible() => false;

        [Header("Fog Settings")] public FloatParameter density = new(0.0f, true);
        public ColorParameter color = new(Color.gray, true);
    }

    public sealed class FogPass : ScriptableRenderPass
    {
        private readonly Shader fogShader = Shader.Find("Hidden/Fog");
        private readonly Material fogMaterial;
        
        private static readonly int Value = Shader.PropertyToID("_Value");
        private static readonly int Color = Shader.PropertyToID("_Color");

        public FogPass()
        {
            fogMaterial = CoreUtils.CreateEngineMaterial(fogShader);
            renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;
            var effect = stack.GetComponent<Fog>();
            if (!effect.IsActive()) return;
            
            var cmd = CommandBufferPool.Get();
            cmd.Clear();
            
            fogMaterial.SetColor(Color, effect.color.value);
            fogMaterial.SetFloat(Value, effect.density.value);

            using (new ProfilingScope(cmd, new ProfilingSampler("Fog Pass")))
            {
                cmd.DrawProcedural(Matrix4x4.identity, fogMaterial, 0, MeshTopology.Triangles, 3);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}