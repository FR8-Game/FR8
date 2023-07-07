using System;
using UnityEngine;

namespace FR8.Player.Submodules
{
    [Serializable]
    public class DiscreteFirstPersonCamera
    {
        private const float GimbalLockOffset = 0.1f;
        private const float YawRange = 180.0f - GimbalLockOffset;
        
        [SerializeField] private float fieldOfView = 70.0f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0.0f, 1.6f, 0.0f);
        
        private PlayerController controller;
        private Transform parent;
        private float yaw;
        
        public Camera Camera { get; private set; }
        
        public void Initialize(PlayerController controller, Transform parent)
        {
            this.controller = controller;
            this.parent = parent;
            
            Camera = Camera.main;
        }

        public void OnEnable()
        {
            var up = parent.up;
            var fwd = parent.forward;
            var right = Vector3.Cross(up, fwd).normalized;
            fwd = Vector3.Cross(right, up).normalized;

            var dot = Mathf.Acos(Vector3.Dot(fwd, parent.forward.normalized)) * Mathf.Rad2Deg;
            yaw = dot;
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        public void Update()
        {
            var delta = controller.LookFrameDelta;
            
            yaw = Mathf.Clamp(yaw + delta.y, -YawRange / 2.0f, YawRange / 2.0f);

            var up = parent.up;
            var fwd = parent.forward;
            var right = Vector3.Cross(up, fwd).normalized;
            fwd = Vector3.Cross(right, up).normalized;

            var orientation = Quaternion.LookRotation(fwd, up);
            orientation *= Quaternion.Euler(0.0f, delta.x, 0.0f) * Quaternion.Euler(-yaw, 0.0f, 0.0f);
            Camera.transform.rotation = orientation;

            Camera.transform.position = parent.position + parent.rotation * cameraOffset;
            Camera.fieldOfView = fieldOfView;
        }
    }
}