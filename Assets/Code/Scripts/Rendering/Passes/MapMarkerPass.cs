using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FR8Runtime.Rendering.Passes
{
    public class MapMarkerPass : CustomRenderPass<MapMarkerSettings>
    {
        private Material material;

        public override bool Enabled => Settings.active;

        public MapMarkerPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (material) return;

            material = new Material(Shader.Find("Unlit/MapMarker"));
            material.hideFlags = HideFlags.HideAndDontSave;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecuteWithCommandBuffer(context, cmd =>
            {
                foreach (var table in MapTable.All)
                {
                    if (!table.Draw) continue;

                    var mapBounds = table.MapBounds;
                    
                    var cam = table.MapCamera;
                    var m0 = Matrix4x4.identity;
                    m0 = Matrix4x4.Translate(-new Vector3(cam.transform.position.x, 0.0f, cam.transform.position.z)) * m0;
                    m0 = Matrix4x4.Rotate(Quaternion.Euler(90.0f, 0.0f, 0.0f) * Quaternion.Inverse(cam.transform.rotation)) * m0;
                    m0 = Matrix4x4.Scale(Vector3.one / cam.orthographicSize) * m0;
                    m0 = table.MapTransform.localToWorldMatrix * m0;

                    var cullMatrix = Matrix4x4.TRS(mapBounds.center, Quaternion.identity, mapBounds.size).inverse * table.MapTransform.worldToLocalMatrix;
                    var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
                    
                    foreach (var marker in MapMarker.All)
                    {
                        if (!marker.Mesh) continue;

                        var meshBounds = marker.Renderer.bounds;

                        if (!GeometryUtility.TestPlanesAABB(frustumPlanes, meshBounds)) continue;
                        
                        var m1 = m0 * marker.transform.localToWorldMatrix;
                        
                        cmd.SetGlobalMatrix("_CullMatrix", cullMatrix);
                        cmd.SetGlobalColor("_MarkerColor", marker.MarkerColor);
                        cmd.DrawMesh(marker.Mesh, m1, material);
                    }
                }
            });
        }
    }
}