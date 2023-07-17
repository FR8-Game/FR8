using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering
{
    public sealed class SelectionOutlinePass : ScriptableRenderPass
    {
        private static readonly int OutlineTargetList = Shader.PropertyToID("_OutlineTarget");

        private Material whiteMaterial;
        private Material blackMaterial;
        private Material blitMaterial;

        public static List<Renderer> ThisFrame { get; } = new();
        public static List<Renderer> Persistant { get; } = new();

        public SelectionOutlinePass()
        {
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
            
            if (!whiteMaterial) whiteMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (!blackMaterial) blackMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (!blitMaterial) blitMaterial = new Material(Shader.Find("Hidden/OutlineBlit"));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!whiteMaterial || !blackMaterial || !blitMaterial) return;
            
            blackMaterial.SetColor("_BaseColor", Color.black);

            var cmd = CommandBufferPool.Get("Selection Outline");
            cmd.Clear();
            cmd.BeginSample("Selection Outline");

            cmd.SetRenderTarget(OutlineTargetList);
            cmd.ClearRenderTarget(true, true, Color.clear);

            var renderListDesc = new RendererListDesc(new ShaderTagId("Unlit"), renderingData.cullResults, renderingData.cameraData.camera);
            var renderList = context.CreateRendererList(renderListDesc);
            cmd.DrawRendererList(renderList);
            
            foreach (var r in ThisFrame) cmd.DrawRenderer(r, whiteMaterial, 0, 0);
            foreach (var r in Persistant) cmd.DrawRenderer(r, whiteMaterial, 0, 0);
            
            cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTarget);
            cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, 0, MeshTopology.Triangles, 3);

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