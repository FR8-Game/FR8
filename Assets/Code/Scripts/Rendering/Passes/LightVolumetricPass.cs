using FR8.Runtime.Rendering.Volumetrics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Runtime.Rendering.Passes
{
    public class LightVolumetricPass : CustomRenderPass<VolumetricsSettings>
    {
        private VolumeLight[] lights;

        public override bool Enabled => Settings.active;

        public LightVolumetricPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            lights = Object.FindObjectsOfType<VolumeLight>();
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecuteWithCommandBuffer(context, cmd =>
            {
                foreach (var l in lights)
                {
                    l.Draw(cmd);
                }
            });
        }
    }
}