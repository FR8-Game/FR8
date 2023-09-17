using System;
using UnityEngine;

namespace FR8Runtime.Player
{
    [DefaultExecutionOrder(200)]
    [SelectionBase, DisallowMultipleComponent]
    public sealed class HUDFX : MonoBehaviour
    {
        [SerializeField] private float sway = 0.15f;
        [SerializeField] private float scale = 1.0f;
        [SerializeField][Range(0.0f, 1.0f)] private float smoothing = 0.8f;

        private Camera mainCam;
        private Vector2 lastTarget;
        private Vector2 position;
        private Vector2 accumulator;
        private Transform renderer;

        private void Awake()
        {
            mainCam = Camera.main;
            renderer = transform.GetChild(0);
        }

        private void FixedUpdate()
        {
            var eulerAngles = mainCam.transform.eulerAngles;
            var target = new Vector2(eulerAngles.y, eulerAngles.x);
            var diff = Delta(target, lastTarget);
            
            accumulator += (diff - accumulator) * Mathf.Lerp(1.0f, Time.deltaTime, smoothing);

            lastTarget = target;
        }

        private void LateUpdate()
        {
            var offset = accumulator * sway;
            transform.rotation = mainCam.transform.rotation * Quaternion.Euler(-offset.y, -offset.x, 0.0f);

            var distanceFromCamera = (renderer.position - mainCam.transform.position).magnitude;
            var angle = mainCam.fieldOfView * Mathf.Deg2Rad / 2.0f;
            var scale = Mathf.Tan(angle) * distanceFromCamera * 2.0f;
            renderer.transform.localScale = new Vector3(scale * (16.0f / 9.0f), scale, scale) * this.scale;
        }

        private Vector2 Delta(Vector2 a, Vector2 b) =>
            new()
            {
                x = Mathf.DeltaAngle(b.x, a.x),
                y = Mathf.DeltaAngle(b.y, a.y)
            };
    }
}