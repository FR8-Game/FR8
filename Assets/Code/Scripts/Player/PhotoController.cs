using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace FR8.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PhotoController : MonoBehaviour
    {
        public float panSpeed;
        public float accelerationTime;
        public float sensitivity = 0.3f;
        public float fovMin = 30.0f;
        public float fovMax = 160.0f;

        [Range(0.0f, 1.0f)]
        public float fovCurrent = 0.5f;

        public float fovSens = 1.0f;
        public int resolutionScale = 1;

        private Vector3 moveTarget;
        private Vector3 velocity;
        private Vector2 cameraRotation;
        private Camera mainCamera;
        private bool screenshotTaken;
        private int cullingMask;

        private static string FileLocation => Path.Combine(Application.dataPath, ".Screenshots");

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            screenshotTaken = false;
            cullingMask = mainCamera.cullingMask;
            mainCamera.cullingMask &= ~(1 << 5);
        }

        private void OnDisable()
        {
            if (screenshotTaken)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = FileLocation,
                    UseShellExecute = true,
                    Verb = "open",
                });
            }

            mainCamera.cullingMask = cullingMask;
        }

        private void Update()
        {
            GetInput();
            UpdatePosition();
            UpdateCamera();
            Capture();
        }

        private void Capture()
        {
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                if (!Directory.Exists(FileLocation)) Directory.CreateDirectory(FileLocation);
                var filename = Path.Combine(FileLocation, $"Screenshot.{DateTime.Now:ss.mm.hh_dd-mm-yy}.png");

                ScreenCapture.CaptureScreenshot(filename, resolutionScale);
                Debug.Log($"Saved Screenshot to \"{filename}\"");
                screenshotTaken = true;
            }
        }

        private void UpdateCamera()
        {
            cameraRotation.x %= 360.0f;
            cameraRotation.y = Mathf.Clamp(cameraRotation.y, -90.0f, 90.0f);

            transform.rotation = Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0.0f);
        }

        private void UpdatePosition()
        {
            var force = (moveTarget - velocity) * Mathf.Min(1.0f / Time.deltaTime, 2.0f / accelerationTime);

            transform.position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
        }

        private void GetInput()
        {
            moveTarget = Vector3.zero;

            var grabbed = Mouse.current.rightButton.isPressed;
            Cursor.lockState = grabbed ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !grabbed;
            if (!grabbed) return;

            cameraRotation += Mouse.current.delta.ReadValue() * sensitivity;

            var kb = Keyboard.current;
            moveTarget = Vector3.ClampMagnitude(transform.TransformVector
            (
                kb.dKey.ReadValue() - kb.aKey.ReadValue(),
                kb.eKey.ReadValue() - kb.qKey.ReadValue(),
                kb.wKey.ReadValue() - kb.sKey.ReadValue()
            ), 1.0f) * panSpeed;

            fovCurrent += Mouse.current.scroll.ReadValue().y * 0.01f * fovSens;
            fovCurrent = Mathf.Clamp01(fovCurrent);
        }

        private void LateUpdate()
        {
            BindToCamera();
        }

        private void BindToCamera()
        {
            mainCamera.transform.position = transform.position;
            mainCamera.transform.rotation = transform.rotation;
            mainCamera.fieldOfView = Mathf.Lerp(fovMin, fovMax, fovCurrent);
        }
    }
}