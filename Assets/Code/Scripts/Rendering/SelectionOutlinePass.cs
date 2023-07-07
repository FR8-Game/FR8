using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering
{
    public sealed class SelectionOutlinePass : ScriptableRenderPass
    {
        private static readonly int OutlineTargetList = Shader.PropertyToID("_OutlineTarget");
        private static readonly int IntermediateTarget = Shader.PropertyToID("_CameraColorAttachmentB");
        private static readonly int FinalTarget = Shader.PropertyToID("_CameraColorAttachmentA");

        private Material renderMaterial;
        private Material blitMaterial;

        public static List<Renderer> ThisFrame { get; } = new();
        public static List<Renderer> Persistant { get; } = new();

        public SelectionOutlinePass()
        {
            renderMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            blitMaterial = new Material(Shader.Find("Hidden/OutlineBlit"));
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public static void RenderThisFrame(GameObject gameObject) => ThisFrame.AddRange(gameObject.GetComponentsInChildren<Renderer>());
        public static void RenderPersistant(GameObject gameObject) => Persistant.AddRange(gameObject.GetComponentsInChildren<Renderer>());

        public static void RemovePersistant(GameObject gameObject)
        {
            foreach (var r in gameObject.GetComponentsInChildren<Renderer>()) Persistant.Remove(r);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(OutlineTargetList, desc, FilterMode.Bilinear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("Selection Outline");
            cmd.Clear();
            cmd.BeginSample("Selection Outline");

            cmd.SetRenderTarget(OutlineTargetList);
            cmd.ClearRenderTarget(true, true, Color.clear);

            foreach (var r in ThisFrame) cmd.DrawRenderer(r, renderMaterial, 0, 0);
            foreach (var r in Persistant) cmd.DrawRenderer(r, renderMaterial, 0, 0);

            cmd.Blit(FinalTarget, IntermediateTarget, blitMaterial);
            cmd.Blit(IntermediateTarget, FinalTarget, blitMaterial);

            cmd.EndSample("Selection Outline");
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            ThisFrame.Clear();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);

            cmd.ReleaseTemporaryRT(OutlineTargetList);
        }
    }
}