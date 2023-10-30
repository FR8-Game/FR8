using System;
using FR8Runtime.Save;
using FR8Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FR8Runtime.Player.Submodules
{
    [Serializable]
    public class PlayerCamera
    {
        private const float GimbalLockOffset = 1.0f;
        private const float YawRange = 180.0f - GimbalLockOffset;

        [SerializeField] private float cameraDrift = 0.0f;
        [SerializeField] private float cameraDriftDeadzone = 0.0f;
        [SerializeField] private float zoomFieldOfView = 35.0f;
        [SerializeField] private float fovSmoothTime = 0.2f;
        [SerializeField] private float nearPlane = 0.2f;
        [SerializeField] private float farPlane = 1000.0f;
        [SerializeField] private CameraShake shakeModule;
        
        private bool zoomCamera;
        private bool cameraLocked;
        private bool wasCameraLocked;

        private float FieldOfView => SaveManager.PersistantSave.GetOrLoad().fieldOfView;
        
        private Vector3 translationOffset;
        private Quaternion rotationalOffset = Quaternion.identity;

        private int lastCursorX, lastCursorY;
        private float fovVelocity;

        private int cursorLockID;

        public Ray LookRay => Avatar.Head ? new(Avatar.Head.position, Avatar.Head.forward) : default;
        public float Yaw { get; private set; }
        public Camera Camera { get; private set; }
        public PlayerAvatar Avatar { get; private set; }

        public void Init(PlayerAvatar avatar)
        {
            Avatar = avatar;

            Camera = Camera.main;

            Avatar.UpdateEvent += Update;
            Avatar.FixedUpdateEvent += FixedUpdate;
            Avatar.EnableEvent += OnEnable;

            cameraLocked = true;
            wasCameraLocked = !cameraLocked;
        }

        private void OnEnable()
        {
            Yaw = Camera.transform.eulerAngles.x;
        }

        private void Update()
        {
            if (Pause.Paused) return;
            if (!Avatar) return;
            if (!Avatar.IsAlive) return;

            if (!Avatar.IsAlive)
            {
                Cursor.lockState = CursorLockMode.None;
                return;
            }

            // Check if cursor is free, or camera has been grabbed.
            if (Avatar.input.FreeCam) cameraLocked = !cameraLocked;
            var cameraLockedThisFrame = cameraLocked || Avatar.input.GrabCam;

            Cursor.lockState = cameraLockedThisFrame ? CursorLockMode.Locked : CursorLockMode.None;

            if (cameraLockedThisFrame != wasCameraLocked)
            {
                if (cameraLockedThisFrame)
                {
                    (lastCursorX, lastCursorY) = CodeUtility.CursorUtility.GetPosition();
                }
                else
                {
                    CodeUtility.CursorUtility.SetPosition(lastCursorX, lastCursorY);
                }
            }

            wasCameraLocked = cameraLockedThisFrame;

            var delta = Vector2.zero;
            if (cameraLockedThisFrame)
            {
                delta += Avatar.input.LookFrameDelta;
            }
            else
            {
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    var normalizedCursorPos = Mouse.current.position.ReadValue() / new Vector2(Screen.width, Screen.height);
                    normalizedCursorPos = normalizedCursorPos * 2.0f - Vector2.one;

                    var drift = cameraDrift * Mathf.Max(0.0f, (normalizedCursorPos.magnitude - cameraDriftDeadzone) / (1.0f - cameraDriftDeadzone));
                    delta += normalizedCursorPos.normalized * drift * Time.deltaTime;
                }
            }

            Yaw = Mathf.Clamp(Yaw + delta.y, -90.0f, 90.0f);
            Camera.transform.rotation = Quaternion.Euler(-Yaw, Camera.transform.eulerAngles.y + delta.x, 0.0f) * rotationalOffset;
            Avatar.Head.rotation = Camera.transform.rotation;
            
            zoomCamera = Avatar.input.ZoomCam;

            // Update additional camera variables.
            Camera.transform.position = Avatar.Head.position + Camera.transform.rotation * translationOffset;
            Camera.fieldOfView = Mathf.SmoothDamp(Camera.fieldOfView, zoomCamera ? zoomFieldOfView : FieldOfView, ref fovVelocity, fovSmoothTime);
            Camera.nearClipPlane = nearPlane;
            Camera.farClipPlane = farPlane;
        }

        private void FixedUpdate()
        {
            shakeModule.GetOffsets(this, out translationOffset, out var rotationalOffset);
        }

        public void SetCameraLock(bool state)
        {
            cameraLocked = state;
        }
        
        public void OnDrawGizmos(PlayerAvatar avatar)
        {
#if UNITY_EDITOR
            Handles.DrawLine(LookRay.GetPoint(0.0f), LookRay.GetPoint(0.5f));
#endif
        }
    }
}