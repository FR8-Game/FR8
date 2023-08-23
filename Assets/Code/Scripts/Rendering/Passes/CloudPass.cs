using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8Runtime.Rendering.Passes
{
    public class CloudPass : CustomRenderPass<CloudSettings>
    {
        public override bool Enabled => Settings.active && Settings.domeMesh.value && Settings.material.value;

        public CloudPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecuteWithCommandBuffer(context, cmd => cmd.DrawMesh(Settings.domeMesh.value, Matrix4x4.identity, Settings.material.value, 0, 0));
        }
    }

    [VolumeComponentMenu("Custom/Clouds")]
    public class CloudSettings : VolumeComponent
    {
        public VolumeParameter<Mesh> domeMesh = new();
        public VolumeParameter<Material> material = new();
    }
}