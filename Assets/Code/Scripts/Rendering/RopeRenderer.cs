using System;
using FR8;
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
        [SerializeField] private float length;

        private Rope rope;
        private new Renderer renderer;
        private MaterialPropertyBlock propertyBlock;

        private static readonly int RopeData = Shader.PropertyToID("_RopeData");

        private Transform StartTarget => startTarget ? startTarget : transform;
        public Transform EndTarget => endTarget ? endTarget : transform;

        private void FixedUpdate()
        {
            UpdateRope();
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

            var start = process(Matrix4x4.TRS(rope.Start, startOrientation, Vector3.one));
            var end = process(Matrix4x4.TRS(rope.End, endOrientation, Vector3.one));
            var mid = process(Matrix4x4.TRS(rope.Mid, Quaternion.Slerp(startOrientation, endOrientation, 0.5f), Vector3.one));

            propertyBlock.SetMatrixArray(RopeData, new[] { start, mid, end });

            Matrix4x4 process(Matrix4x4 m) => transform.worldToLocalMatrix * m * transform.localToWorldMatrix;
        }

        private void OnValidate()
        {
            length = Mathf.Max(0.0f, length);
        }
    }
}