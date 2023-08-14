﻿using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FR8.Rendering.Volumetrics
{
    [RequireComponent(typeof(Light))]
    public class VolumeLight : MonoBehaviour
    {
        [SerializeField] private float density = 0.2f;
        [SerializeField] private int resolution = 50;
        
        private Material material;
        private Light light;
        private static readonly int LightData = Shader.PropertyToID("_LightData");
        private static readonly int LightColor = Shader.PropertyToID("_LightColor");
        private static readonly int Density = Shader.PropertyToID("_Density");
        private static readonly int Resolution = Shader.PropertyToID("_Resolution");

        public void Draw(CommandBuffer cmd)
        {
            if (!material) GetMaterial();
            if (!light) light = GetComponent<Light>();

            var innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            var outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.spotAngle);
            var angleRangeInv = 1.0f / Mathf.Max(innerCos - outerCos, 0.001f);
            
            var lightData = light.type switch
            {
                LightType.Spot => new Vector4
                {
                    x = light.range,
                    y = angleRangeInv,
                    z = -outerCos * angleRangeInv,
                },
                LightType.Directional => Vector4.zero,
                LightType.Point => new Vector4
                {
                    x = light.range,
                    y = 0.0f,
                    z = 1.0f,
                },
                LightType.Area => Vector4.zero,
                LightType.Disc => Vector4.zero,
                _ => throw new ArgumentOutOfRangeException()
            };
            material.SetVector(LightData, lightData);
            material.SetFloat(Density, density);
            material.SetInt(Resolution, resolution);

            var lightColor = light.color * light.intensity * (light.useColorTemperature ? Mathf.CorrelatedColorTemperatureToRGB(light.colorTemperature) : Color.white);
            material.SetVector(LightColor, lightColor);

            cmd.DrawProcedural(transform.localToWorldMatrix, material, -1, MeshTopology.Triangles, 6 * resolution);
        }

        private void GetMaterial()
        {
            var shader = Shader.Find("Hidden/Volumetrics");
            material = new Material(shader);
            material.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}