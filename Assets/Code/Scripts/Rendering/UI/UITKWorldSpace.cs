using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

namespace FR8Runtime.Rendering.UI
{
    [ExecuteAlways]
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument), typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class UITKWorldSpace : MonoBehaviour
    {
        [SerializeField] private string textureParameterName = "_BaseColor";

        [Space]
        [SerializeField] private int width = 1280;
        [SerializeField] private int height = 720;

        [Space]
        [SerializeField] private bool driveScale;
        [SerializeField] private float baseScale = 1.0f;

        private MeshRenderer renderer;
        private UIDocument document;
        private RenderTexture targetTexture;
        private bool initialized;
        
        private MaterialPropertyBlock propertyBlock;

        private void OnEnable()
        {
            initialized = false;

            document = GetComponent<UIDocument>();
            if (!document.visualTreeAsset) return;
            if (width <= 1) return;
            if (height <= 1) return;

            initialized = true;
            document.enabled = true;
            
            renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.enabled = true;

            document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            document.panelSettings.hideFlags = HideFlags.HideAndDontSave;

            targetTexture = new RenderTexture(width, height, GraphicsFormat.R32G32B32A32_SFloat, GraphicsFormat.None);
            targetTexture.name = $"{GetType().Name} Render Target [{width}, {height}]";
            targetTexture.hideFlags = HideFlags.HideAndDontSave;
            document.panelSettings.targetTexture = targetTexture;

            propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetTexture(textureParameterName, targetTexture);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private void OnDisable()
        {
            if (!initialized) return;

            renderer.enabled = false;
            document.enabled = false;
            
            DestroyImmediate(document.panelSettings);
            document.panelSettings = null;
            
            DestroyImmediate(targetTexture);

            initialized = false;
        }

        private void OnValidate()
        {
            width = Mathf.Max(2, width);
            height = Mathf.Max(2, height);

            if (driveScale)
            {
                transform.localScale = new Vector3(1.0f, height / (float)width, 1.0f) * baseScale;
            }
        }
    }
}