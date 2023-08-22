using FR8Runtime.CodeUtility;
using UnityEngine;

namespace FR8Runtime.Rendering
{
    [ExecuteAlways]
    public class RopeRenderer : MonoBehaviour
    {
        [Space]
        [SerializeField] private Transform startTarget;

        [SerializeField] private Vector3 startTranslation;
        [SerializeField] private Vector3 startRotation;

        [Space]
        [SerializeField] private Transform endTarget;
        [SerializeField] private Vector3 endTranslation;
        [SerializeField] private Vector3 endRotation;

        [Space]
        [SerializeField] private Rope rope = new();
        [SerializeField] private float length;
        [SerializeField] private float size = 1.0f;
        [SerializeField] private float twist;

        private new Renderer renderer;
        private MaterialPropertyBlock propertyBlock;

        private static readonly int RopeStart = Shader.PropertyToID("_RopeStart");
        private static readonly int RopeMid = Shader.PropertyToID("_RopeMid");
        private static readonly int RopeEnd = Shader.PropertyToID("_RopeEnd");
        private static readonly int IsRope = Shader.PropertyToID("_IsRope");
        private static readonly int RopeTwist = Shader.PropertyToID("_Twist");

        private Transform StartTarget => startTarget ? startTarget : transform;
        public Transform EndTarget => endTarget ? endTarget : transform;

        private void FixedUpdate()
        {
            if (Application.isPlaying)
            {
                UpdateRope();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateRope();
            }
        }

        private void UpdateRope()
        {
            var start = StartTarget.TransformPoint(startTranslation);
            var end = EndTarget.TransformPoint(endTranslation);
            var length = (end - start).magnitude + Mathf.Max(0.0f, this.length);

            rope.Update(start, end, length);
        }

        private void OnRenderObject()
        {
            if (!renderer) renderer = GetComponent<MeshRenderer>();
            if (!renderer) return;

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
                renderer.SetPropertyBlock(propertyBlock);
            }

            var startOrientation = StartTarget.rotation * Quaternion.Euler(startRotation);
            var endOrientation = EndTarget.rotation * Quaternion.Euler(endRotation);

            var scale = Vector3.one * Mathf.Exp(size);
            
            var start = Matrix4x4.TRS(rope.Start, startOrientation, scale);
            var end = Matrix4x4.TRS(rope.End, endOrientation, scale);
            var mid = Matrix4x4.TRS(rope.Mid, Quaternion.Slerp(startOrientation, endOrientation, 0.5f), scale);
            
            propertyBlock.SetFloat(IsRope, 1.0f);
            propertyBlock.SetFloat(RopeTwist, twist);
            propertyBlock.SetMatrix(RopeStart, transform.worldToLocalMatrix * start);
            propertyBlock.SetMatrix(RopeMid, transform.worldToLocalMatrix * mid);
            propertyBlock.SetMatrix(RopeEnd, transform.worldToLocalMatrix * end);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private void OnValidate()
        {
            length = Mathf.Max(0.0f, length);
        }

        private void OnDrawGizmos()
        {
            rope.DrawGizmos();
        }
    }
}