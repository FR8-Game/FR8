using FR8.Rendering.Volumetrics;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace FR8Runtime.Rendering.Passes
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

    [VolumeComponentMenu("Custom/Volumetrics")]
    public class VolumetricsSettings : VolumeComponent { }
}