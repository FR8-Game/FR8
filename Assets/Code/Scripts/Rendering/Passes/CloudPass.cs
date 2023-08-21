using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering.Passes
{
    public class CloudPass : CustomRenderPass
    {
        public Settings settings;

        public override bool Enabled => settings.enabled && settings.domeMesh && settings.material;

        public CloudPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecuteWithCommandBuffer(context, cmd => cmd.DrawMesh(settings.domeMesh, Matrix4x4.identity, settings.material, 0, 0));
        }

        [System.Serializable]
        public class Settings
        {
            public bool enabled;
            public Mesh domeMesh;
            public Material material;
        }
    }
}