// using FR8.Rendering.Volumetrics;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;
//
// namespace FR8.Rendering.Passes
// {
//     public class LightVolumetricPass : ScriptableRenderPass
//     {
//         private VolumeLight[] lights;
//         
//         public LightVolumetricPass()
//         {
//             renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
//         }
//         
//         public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//         {
//             lights = Object.FindObjectsOfType<VolumeLight>();
//         }
//
//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//         {
//             var cmd = CommandBufferPool.Get("Volumetrics");
//             cmd.Clear();
//             
//             foreach (var l in lights)
//             {
//                 l.Draw(cmd);
//             }
//             
//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }
//     }
// }