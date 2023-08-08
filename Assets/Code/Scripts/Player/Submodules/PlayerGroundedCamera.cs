using System;
using UnityEngine;
using Cursor = FR8.Utility.Cursor;

namespace FR8.Player.Submodules
{
    [Serializable]
    public class PlayerGroundedCamera
    {
        private const float GimbalLockOffset = 0.1f;
        private const float YawRange = 180.0f - GimbalLockOffset;

        [SerializeField] private float fieldOfView = 70.0f;
        [SerializeField] private float zoomFieldOfView = 35.0f;
        [SerializeField] private float fovSmoothTime = 0.2f;
        [SerializeField] private float nearPlane = 0.2f;
        [SerializeField] private float farPlane = 1000.0f;
        [SerializeField] private CameraShake shakeModule;

        private Func<PlayerController> controller;
        private Transform target;
        private Vector2 delta;
        private float yaw;
        private bool zoomCamera;
        private bool cameraLocked;
        private bool wasCameraLocked;

        private Quaternion orientation;
        private Vector3 translationOffset;

        private int lastCursorX, lastCursorY;
        private float fovVelocity;

        private int cursorLockID;

        public Camera Camera { get; private set; }
        public PlayerController Controller => controller();

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

            var cameraLockedThisFrame = cameraLocked || controller.GrabCam;
            if (cameraLockedThisFrame != wasCameraLocked)
            {
#if UNITY_EDITOR
                Cursor.Change(cursorLockID, cameraLockedThisFrame ? CursorLockMode.Locked : CursorLockMode.None);
#else
                Cursor.Change(cursorLockID, cameraLocked ? CursorLockMode.Locked : CursorLockMode.Confined);
#endif
                if (cameraLockedThisFrame)
                {
                    (lastCursorX, lastCursorY) = Cursor.GetPosition();
                }
                else
                {
                    Cursor.SetPosition(lastCursorX, lastCursorY);
                }
            }
            
            wasCameraLocked = cameraLockedThisFrame;
            delta += controller.LookFrameDelta;

            zoomCamera = controller.ZoomCam;
            
            target.transform.rotation = orientation;
            Camera.transform.rotation = orientation;
            
             // Update additional camera variables.
            Camera.transform.position = target.position + Camera.transform.rotation * translationOffset;
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, zoomCamera ? zoomFieldOfView : fieldOfView, ref fovVelocity, fovSmoothTime);
            Camera.nearClipPlane = nearPlane;
            Camera.farClipPlane = farPlane;
        }

        public void FixedUpdate()
        {
            // Get delta rotation input from controller
            var delta = this.delta;
            this.delta = Vector2.zero;
            
            // Apply input and clamp camera's yaw
            yaw = Mathf.Clamp(yaw + delta.y, -YawRange / 2.0f, YawRange / 2.0f);
            
            shakeModule.GetOffsets(this, out translationOffset, out var rotationalOffset);
            orientation = Quaternion.Euler(-yaw, orientation.eulerAngles.y + delta.x, 0.0f) * rotationalOffset;
        }
        
        public void SetCameraLock(bool state)
        {
            cameraLocked = state;
        }
    }
}