using System;
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

                    foreach (var marker in MapMarker.All)
                    {
                        if (!marker.Mesh) continue;

                        var cam = table.MapCamera;
                        var matrix = marker.transform.localToWorldMatrix;
                        matrix = Matrix4x4.Translate(-new Vector3(cam.transform.position.x, 0.0f, cam.transform.position.z)) * matrix;
                        matrix = Matrix4x4.Rotate(Quaternion.Euler(90.0f, 0.0f, 0.0f) * Quaternion.Inverse(cam.transform.rotation)) * matrix;
                        matrix = Matrix4x4.Scale(Vector3.one / cam.orthographicSize) * matrix;
                        matrix = table.MapTransform.localToWorldMatrix * matrix;

                        var cullMatrix = Matrix4x4.TRS(table.MapBounds.center, Quaternion.identity, table.MapBounds.size).inverse * table.MapTransform.worldToLocalMatrix;

                        cmd.SetGlobalMatrix("_CullMatrix", cullMatrix);
                        cmd.SetGlobalColor("_MarkerColor", marker.MarkerColor);
                        cmd.DrawMesh(marker.Mesh, matrix, material);
                    }
                }
            });
        }

    }
        [VolumeComponentMenu("Custom/Map Markers")]
        public class MapMarkerSettings : VolumeComponent
        {
            public FloatParameter markerScale = new(1.0f);
        }
}