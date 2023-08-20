using System;
using UnityEngine;
using Cursor = FR8.Utility.Cursor;

namespace FR8.Player.Submodules
{
    [Serializable]
    public class PlayerCamera
    {
        private const float GimbalLockOffset = 1.0f;
        private const float YawRange = 180.0f - GimbalLockOffset;

        public Vector3 cameraOffset = new(0.0f, 1.6f, 0.0f);
        [SerializeField] private float fieldOfView = 70.0f;
        [SerializeField] private float zoomFieldOfView = 35.0f;
        [SerializeField] private float fovSmoothTime = 0.2f;
        [SerializeField] private float nearPlane = 0.2f;
        [SerializeField] private float farPlane = 1000.0f;
        [SerializeField] private CameraShake shakeModule;

        private Vector2 delta;
        private bool zoomCamera;
        private bool cameraLocked;
        private bool wasCameraLocked;

        private Vector3 translationOffset;

        private int lastCursorX, lastCursorY;
        private float fovVelocity;

        private int cursorLockID;

        public Transform CameraTarget { get; private set; }
        public Ray LookingRay => new(CameraTarget.position, CameraTarget.forward);
        public float Yaw { get; private set; }
        public Camera Camera { get; private set; }
        public PlayerAvatar Avatar { get; private set; }

        public void Init(PlayerAvatar avatar)
        {
            this.Avatar = avatar;

            Camera = Camera.main;

            Avatar.EnabledEvent += OnEnable;
            Avatar.DisabledEvent += OnDisable;
            Avatar.UpdateEvent += Update;
            Avatar.FixedUpdateEvent += FixedUpdate;
            
            CameraTarget = avatar.transform.Find("Camera Target");
            CameraTarget.transform.localPosition = cameraOffset;
            CameraTarget.transform.localRotation = Quaternion.identity;
        }

        private void OnEnable()
        {
            cameraLocked = true;
            Cursor.Push(CursorLockMode.Locked, ref cursorLockID);
        }

        private void OnDisable()
        {
            Cursor.Pop(ref cursorLockID);
        }

        private void Update()
        {
            if (!Avatar) return;
            
            // Check if cursor is free, or camera has been grabbed.
            if (Avatar.input.FreeCam) cameraLocked = !cameraLocked;

            var cameraLockedThisFrame = cameraLocked || Avatar.input.GrabCam;
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
            delta += Avatar.input.LookFrameDelta;

            zoomCamera = Avatar.input.ZoomCam;
            
            Camera.transform.rotation = CameraTarget.transform.rotation;
            
             // Update additional camera variables.
            Camera.transform.position = CameraTarget.position + Camera.transform.rotation * translationOffset;
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, zoomCamera ? zoomFieldOfView : fieldOfView, ref fovVelocity, fovSmoothTime);
            Camera.nearClipPlane = nearPlane;
            Camera.farClipPlane = farPlane;
        }

        private void FixedUpdate()
        {
            // Get delta rotation input from controller
            var delta = this.delta;
            this.delta = Vector2.zero;
            
            // Apply input and clamp camera's yaw
            Yaw = Mathf.Clamp(Yaw + delta.y, -YawRange / 2.0f, YawRange / 2.0f);
            
            shakeModule.GetOffsets(this, out translationOffset, out var rotationalOffset);
            CameraTarget.transform.rotation = Quaternion.Euler(-Yaw, CameraTarget.transform.eulerAngles.y + delta.x, 0.0f) * rotationalOffset;
        }
        
        public void SetCameraLock(bool state)
        {
            cameraLocked = state;
        }
    }
}