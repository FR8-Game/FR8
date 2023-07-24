using System;
using UnityEngine;
using Cursor = FR8.Utility.Cursor;

namespace FR8.Player.Submodules
{
    [Serializable]
    public class DiscreteFirstPersonCamera
    {
        private const float GimbalLockOffset = 0.1f;
        private const float YawRange = 180.0f - GimbalLockOffset;
        
        [SerializeField] private float fieldOfView = 70.0f;
        [SerializeField] private float zoomFieldOfView = 35.0f;
        [SerializeField] private float fovSmoothTime = 0.2f;
        [SerializeField] private float nearPlane = 0.2f;
        [SerializeField] private float farPlane = 1000.0f;
        
        private Func<PlayerController> controller;
        private Transform target;
        private float yaw;
        private bool cameraLocked;
        
        private float fovVelocity;

        private int cursorLockID;
        
        public Camera Camera { get; private set; }
        
        public void Initialize(Func<PlayerController> controller, Transform target)
        {
            this.controller = controller;
            this.target = target;
            
            Camera = Camera.main;
        }

        public void OnEnable()
        {
            cameraLocked = true;
            Cursor.Push(CursorLockMode.Locked, ref cursorLockID);
        }

        public void OnDisable()
        {
            Cursor.Pop(ref cursorLockID);
        }
        
        public void Update()
        {
            var controller = this.controller();
            if (!controller) return;
            
            // Check if cursor is free, or camera has been grabbed.
            if (controller.FreeCam) cameraLocked = !cameraLocked;

            if (controller.GrabCam)
            {
                Cursor.Change(cursorLockID, CursorLockMode.Locked);
            }
            else
            {
                Cursor.Change(cursorLockID, cameraLocked ? CursorLockMode.Locked : CursorLockMode.Confined);
            }

            // Get delta rotation input from controller
            var delta = controller.LookFrameDelta;
            
            // Apply input and clamp camera's yaw
            yaw = Mathf.Clamp(yaw + delta.y, -YawRange / 2.0f, YawRange / 2.0f);

            target.transform.rotation = Quaternion.Euler(-yaw, target.transform.eulerAngles.y + delta.x, 0.0f);
            Camera.transform.rotation = target.transform.rotation;

            // Update additional camera variables.
            Camera.transform.position = target.position;
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, controller.ZoomCam ? zoomFieldOfView : fieldOfView, ref fovVelocity, fovSmoothTime);
            Camera.nearClipPlane = nearPlane;
            Camera.farClipPlane = farPlane;
        }

        public void SetCameraLock(bool state)
        {
            cameraLocked = state;
        }
    }
}