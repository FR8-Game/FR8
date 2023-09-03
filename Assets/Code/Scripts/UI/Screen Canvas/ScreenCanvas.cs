using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace FR8Runtime.UI.Screen_Canvas
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class ScreenCanvas : MonoBehaviour
    {
        private const int RenderLayer = 11;

        private Renderer target;
        private Vector2Int resolution;

        private Camera renderCamera;
        private RenderTexture renderTexture;
        private MaterialPropertyBlock propertyBlock;

        private static readonly int MainTexKey = Shader.PropertyToID("_MainTex");
        private static readonly int ResolutionKey = Shader.PropertyToID("_Resolution");
        private static readonly int UVMatrixKey = Shader.PropertyToID("_UVMatrix");

        private void Start()
        {
            target = GetComponentInParent<Renderer>();
            if (!target)
            {
                Destroy(this);
                return;
            }

            gameObject.layer = RenderLayer;

            var cam = Camera.main;
            if (cam) cam.cullingMask &= ~(0b1 << RenderLayer);

            renderCamera = new GameObject($"{name}.RenderCamera").AddComponent<Camera>();
            renderCamera.cullingMask = 0b1 << RenderLayer;

            renderCamera.transform.position = Vector3.up * 100.0f;
            renderCamera.transform.rotation = Quaternion.identity;
            renderCamera.transform.localScale = Vector3.one;

            renderCamera.orthographic = true;
            renderCamera.orthographicSize = 1.0f;
            renderCamera.nearClipPlane = 0.0f;
            renderCamera.farClipPlane = 2.0f;

            renderCamera.backgroundColor = Color.clear;
            renderCamera.clearFlags = CameraClearFlags.SolidColor;

            renderCamera.hideFlags = HideFlags.HideAndDontSave;

            var canvas = GetComponent<Canvas>();
            resolution = Vector2Int.RoundToInt(((RectTransform)canvas.transform).rect.size);
            
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 1.0f;
            canvas.worldCamera = renderCamera;

            propertyBlock = new MaterialPropertyBlock();

            transform.SetParent(null);

            UpdateRenderTexture();
        }

        private void OnDisable()
        {
            if (renderTexture)
            {
                Destroy(renderTexture);
                renderTexture = null;
            }

            propertyBlock.SetTexture(MainTexKey, null);
        }

        private void Update()
        {
            UpdateRenderTexture();
            propertyBlock.SetTexture(MainTexKey, renderTexture);
            propertyBlock.SetVector(ResolutionKey, (Vector2)resolution);
            if (target) target.SetPropertyBlock(propertyBlock);
        }

        private void UpdateRenderTexture()
        {
            if (renderTexture)
            {
                if (renderTexture.width == resolution.x && renderTexture.height == resolution.y) return;

                Destroy(renderTexture);
            }

            renderTexture = new RenderTexture(resolution.x, resolution.y, GraphicsFormat.R32G32B32A32_SFloat, GraphicsFormat.D32_SFloat_S8_UInt);
            renderTexture.hideFlags = HideFlags.HideAndDontSave;
            renderCamera.targetTexture = renderTexture;
        }
    }
}