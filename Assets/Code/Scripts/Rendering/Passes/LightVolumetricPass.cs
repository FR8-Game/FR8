using System;
using FR8.Rendering.Volumetrics;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace FR8.Rendering.Passes
{
    public class LightVolumetricPass : CustomRenderPass
    {
        public Settings settings;

        private VolumeLight[] lights;

        public override bool Enabled => settings.enabled;

        public LightVolumetricPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            lights = Object.FindObjectsOfType<VolumeLight>();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecuteWithCommandBuffer(context, cmd =>
            {
                foreach (var l in lights)
                {
                    l.Draw(cmd);
                }
            });
        }

        [Serializable]
        public class Settings
        {
            public bool enabled;
        }
    }
}