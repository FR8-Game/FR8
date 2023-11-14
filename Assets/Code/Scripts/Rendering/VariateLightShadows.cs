using UnityEditor;
using UnityEngine;
using ColorUtility = HBCore.Utility.ColorUtility;

namespace FR8.Runtime.Rendering
{
    [RequireComponent(typeof(Light))]
    public class VariateLightShadows : MonoBehaviour
    {
        [SerializeField] private float shadowCullDistance = 5.0f;
        [SerializeField] private float shadowFadeDistance = 1.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float shadowMaxStrength = 1.0f;
        [SerializeField] private LightShadows mode = LightShadows.Soft;

        private Camera mainCamera;
        private new Light light;

        private float Inner => shadowCullDistance;
        private float Outer => shadowCullDistance + shadowFadeDistance;

        private void Awake()
        {
            mainCamera = Camera.main;
            light = GetComponent<Light>();
        }

        private void Update()
        {
            var dist = transform.InverseTransformPoint(mainCamera.transform.position).magnitude;
            light.shadowStrength = Mathf.InverseLerp(Outer, Inner, dist) * shadowMaxStrength;
            light.shadows = dist < Outer ? mode : LightShadows.None;
        }

        private void OnValidate()
        {
            shadowCullDistance = Mathf.Max(0.0f, shadowCullDistance);
            shadowFadeDistance = Mathf.Max(0.0f, shadowFadeDistance);
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject != gameObject) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = ColorUtility.Gray(1.0f);
            Gizmos.DrawWireSphere(Vector3.zero, Inner);
            Gizmos.color = ColorUtility.Gray(1.0f, 0.2f);
            Gizmos.DrawWireSphere(Vector3.zero, Outer);
#endif
        }
    }
}