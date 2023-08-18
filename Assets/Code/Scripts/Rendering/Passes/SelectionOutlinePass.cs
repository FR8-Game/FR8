using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

namespace FR8.Rendering.Passes
{
    public sealed class SelectionOutlinePass// : ScriptableRenderPass
    {
        private static readonly int OutlineTarget = Shader.PropertyToID("_OutlineTarget");

        private Material whiteMaterial;
        private Material blackMaterial;
        private Material blitMaterial;

        public static List<Renderer> ThisFrame { get; } = new();
        public static List<Renderer> Persistant { get; } = new();

        // public SelectionOutlinePass()
        // {
        //     renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        // }

        public static void RenderThisFrame(GameObject gameObject) => ThisFrame.AddRange(gameObject.GetComponentsInChildren<Renderer>());
        public static void RenderPersistant(GameObject gameObject) => Persistant.AddRange(gameObject.GetComponentsInChildren<Renderer>());

        public static void RemovePersistant(GameObject gameObject)
        {
            foreach (var r in gameObject.GetComponentsInChildren<Renderer>()) Persistant.Remove(r);
        }

        // public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        // {
        //     base.OnCameraSetup(cmd, ref renderingData);
        //
        //     var desc = renderingData.cameraData.cameraTargetDescriptor;
        //     desc.depthBufferBits = 32;
        //     desc.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
        //     cmd.GetTemporaryRT(OutlineTarget, desc, FilterMode.Bilinear);
        //
        //     if (!whiteMaterial) whiteMaterial = new Material(Shader.Find("Unlit/OutlineObject"));
        //     if (!blackMaterial) blackMaterial = new Material(Shader.Find("Unlit/OutlineObject"));
        //     if (!blitMaterial) blitMaterial = new Material(Shader.Find("Hidden/OutlineBlit"));
        //
        //     Persistant.RemoveAll(e => !e);
        //     Persistant.RemoveAll(e => e.CompareTag("Do Not Outline"));
        //     ThisFrame.RemoveAll(e => e.CompareTag("Do Not Outline"));
        // }
        //
        // public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        // {
        //     if (!whiteMaterial) return;
        //     if (!blackMaterial) return;
        //     if (!blitMaterial) return;
        //     
        //     blackMaterial.SetColor("_BaseColor", Color.black);
        //
        //     var cmd = CommandBufferPool.Get("Selection Outline");
        //     cmd.Clear();
        //     cmd.BeginSample("Selection Outline");
        //
        //     cmd.SetRenderTarget(OutlineTarget);
        //     cmd.ClearRenderTarget(true, true, Color.clear);
        //
        //     foreach (var r in ThisFrame)
        //     {
        //         DrawRenderer(cmd, r);
        //     }
        //
        //     foreach (var r in Persistant)
        //     {
        //         DrawRenderer(cmd, r);
        //     }
        //     
        //     cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTarget);
        //     cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, 0, MeshTopology.Triangles, 3);
        //
        //     cmd.EndSample("Selection Outline");
        //     context.ExecuteCommandBuffer(cmd);
        //     CommandBufferPool.Release(cmd);
        //
        //     ThisFrame.Clear();
        // }
        //
        // private void DrawRenderer(CommandBuffer cmd, Renderer renderer)
        // {
        //     for (var i = 0; i < renderer.sharedMaterials.Length; i++)
        //     {
        //         cmd.DrawRenderer(renderer, whiteMaterial, i, 0);
        //     }
        // }
        //
        // public override void OnCameraCleanup(CommandBuffer cmd)
        // {
        //     base.OnCameraCleanup(cmd);
        //
        //     cmd.ReleaseTemporaryRT(OutlineTarget);
        // }
    }
}