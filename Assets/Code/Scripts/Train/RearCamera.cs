using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace FR8Runtime
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class RearCamera : MonoBehaviour
    {
        [SerializeField] private int materialIndex = 1;
        [SerializeField] private int width = 640;
        [SerializeField] private int height = 360;
        [SerializeField] private int framerate = 12;
        [SerializeField] private string materialParameterName = "_MainTex";

        private MeshRenderer renderer;
        private Material material;
        private Camera renderCamera;
        private RenderTexture rt;
        private float timer;

        private void OnEnable()
        {
            if (!material)
            {
                renderer = GetComponentInChildren<MeshRenderer>();
                material = renderer.materials[materialIndex];
            }

            renderCamera = GetComponentInChildren<Camera>();
            rt = new RenderTexture(width, height, 24, DefaultFormat.HDR);
            rt.filterMode = FilterMode.Point;
            
            renderCamera.targetTexture = rt;
            renderCamera.enabled = false;
            material.SetTexture(materialParameterName, rt);
        }

        private void OnDisable()
        {
            rt.Release();
            Destroy(rt);
        }

        private void Update()
        {
            while (timer > 1.0f / framerate)
            {
                renderCamera.Render();
                timer -= 1.0f / framerate;
            }
            
            timer += Time.deltaTime;
        }
    }
}