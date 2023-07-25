using UnityEngine;
using UnityEngine.UI;

namespace FR8.Rendering
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RenderTexture))]
    public class Mirror : MonoBehaviour
    {
        [SerializeField] private float textureScaling = 1.0f;
        [SerializeField] private float nearPlaneOffset = -0.5f;
        [SerializeField] private float nearPlaneMin = 0.01f;

        private Shader shader;
        private RawImage display;
        private Material material;
        private RenderTexture rt;
        private Camera mainCamera;
        private Camera mirrorCamera;

        private int Width => Mathf.RoundToInt(Screen.width * textureScaling);
        private int Height => Mathf.RoundToInt(Screen.height * textureScaling);
        
        private void Awake()
        {
            mainCamera = Camera.main;

            shader = Shader.Find("Hidden/Mirror");
            material = new Material(shader);
            material.name = "[PROC] Mirror";
            material.hideFlags = HideFlags.HideAndDontSave;
            
            mirrorCamera = new GameObject("Mirror Camera").AddComponent<Camera>();
            mirrorCamera.transform.SetParent(transform);
            mirrorCamera.depth = -1;

            display = GetComponent<RawImage>();
            display.material = material;
        }

        private void LateUpdate()
        {
            UpdateRenderTexture();

            var vector = mainCamera.transform.position - transform.position;
            vector = reflect(vector);
            var position = vector + transform.position;

            var forward = reflect(mainCamera.transform.forward);
            var rotation = Quaternion.LookRotation(forward, Vector3.up);

            mirrorCamera.transform.position = position;
            mirrorCamera.transform.rotation = rotation;

            var rectHalfWidth = display.rectTransform.rect.width / 2.0f;
            var rectHalfHeight = display.rectTransform.rect.height / 2.0f;
            Vector3[] corners =
            {
                transform.TransformPoint( rectHalfWidth,  rectHalfHeight, 0.0f),
                transform.TransformPoint(-rectHalfWidth,  rectHalfHeight, 0.0f),
                transform.TransformPoint( rectHalfWidth, -rectHalfHeight, 0.0f),
                transform.TransformPoint(-rectHalfWidth, -rectHalfHeight, 0.0f),
            };
            mirrorCamera.nearClipPlane = nearPlaneMin;
            for (var i = 0; i < 3; i++) mirrorCamera.nearClipPlane = Mathf.Max(mirrorCamera.nearClipPlane, (corners[i] - position).magnitude + nearPlaneOffset);
            
            mirrorCamera.fieldOfView = mainCamera.fieldOfView;
            mirrorCamera.ResetProjectionMatrix();
            mirrorCamera.ResetWorldToCameraMatrix();
            material.SetMatrix("_MirrorVP", mirrorCamera.projectionMatrix * mirrorCamera.worldToCameraMatrix);
            
            Vector3 reflect(Vector3 vector) => Vector3.Reflect(vector, transform.forward);
        }

        private void UpdateRenderTexture()
        {
            RenderTexture create() => new(Width, Height, 32, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default);

            if (rt)
            {
                if (rt.width == Width && rt.height == Height) return;

                Destroy(rt);
                rt = create();
            }
            else
            {
                rt = create();
            }
            
            mirrorCamera.targetTexture = rt;
            display.texture = rt;
        }

        private void OnValidate()
        {
            textureScaling = Mathf.Max(0.1f, textureScaling);
        }
    }
}