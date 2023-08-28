using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Rendering
{
    [ExecuteAlways]
    public class RopeRenderer : MonoBehaviour
    {
        [Space]
        public Transform startTarget;

        public Vector3 startTranslation;
        public Vector3 startRotation;

        [Space]
        public Transform endTarget;
        public Vector3 endTranslation;
        public Vector3 endRotation;

        [Space]
        public CodeUtility.Rope rope = new();
        public float slack;
        public float lengthSmoothing;
        public float size = 1.0f;
        public float twist;

        private float length;

        private new Renderer renderer;
        private MaterialPropertyBlock propertyBlock;

        private static readonly int RopeStart = Shader.PropertyToID("_RopeStart");
        private static readonly int RopeMid = Shader.PropertyToID("_RopeMid");
        private static readonly int RopeEnd = Shader.PropertyToID("_RopeEnd");
        private static readonly int IsRope = Shader.PropertyToID("_IsRope");
        private static readonly int RopeTwist = Shader.PropertyToID("_Twist");

        private Transform StartTarget => startTarget ? startTarget : transform;
        public Transform EndTarget => endTarget ? endTarget : transform;

        private void OnEnable()
        {
            if (!renderer) renderer = GetComponent<MeshRenderer>();
            if (renderer) renderer.enabled = true;
            
        }

        private void OnDisable()
        {
            if (!renderer) renderer = GetComponent<MeshRenderer>();
            if (renderer) renderer.enabled = false;
        }

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
            var tLength = (end - start).magnitude + Mathf.Max(0.0f, slack);

            if (lengthSmoothing > Time.deltaTime) length += (tLength - length) * Mathf.Min(Time.deltaTime / lengthSmoothing, 1.0f);
            else length = tLength;
            
            if (tLength > length) length = tLength;
            
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

            var bounds = new Bounds(rope.Start, Vector3.zero);
            bounds.Encapsulate(rope.Mid);
            bounds.Encapsulate(rope.End);
            bounds.Expand(1.0f);
            renderer.bounds = bounds;
        }

        private void OnValidate()
        {
            slack = Mathf.Max(0.0f, slack);
        }

        private void OnDrawGizmos()
        {
            rope.DrawGizmos();
        }
    }
}