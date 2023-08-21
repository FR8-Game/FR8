using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8.Rendering.Passes
{
    [Serializable]
    public abstract class CustomRenderPass : ScriptableRenderPass
    {
        public abstract bool Enabled { get; }

        public void Enqueue(ScriptableRenderer renderer)
        {
            if (!Enabled) return;
            renderer.EnqueuePass(this);
        }

        protected void ExecuteWithCommandBuffer(ScriptableRenderContext context, Action<CommandBuffer> callback)
        {
            var name = GetType().Name;
            
            var cmd = CommandBufferPool.Get(name);
            cmd.Clear();
            cmd.BeginSample(name);

            using (new ProfilingScope(cmd, new ProfilingSampler(name)))
            {
                callback(cmd);
            }

            cmd.EndSample(name);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}