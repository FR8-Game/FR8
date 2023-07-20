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
        [SerializeField] private Vector3 cameraOffset = new(0.0f, 1.6f, 0.0f);
        
        private Func<PlayerController> controller;
        private Transform parent;
        private float yaw;
        private bool cameraLocked;

        private int cursorLockID;
        
        public Camera Camera { get; private set; }
        
        public void Initialize(Func<PlayerController> controller, Transform parent)
        {
            this.controller = controller;
            this.parent = parent;
            
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

            parent.transform.rotation *= Quaternion.Euler(0.0f, delta.x, 0.0f);
            var cameraOrientation = parent.transform.rotation * Quaternion.Euler(-yaw, 0.0f, 0.0f);
            Camera.transform.rotation = cameraOrientation;

            // Update additional camera variables.
            Camera.transform.position = parent.position + parent.rotation * cameraOffset;
            Camera.fieldOfView = fieldOfView;
        }

        public void SetCameraLock(bool state)
        {
            cameraLocked = state;
        }
    }
}