using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FR8.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PhotoController : MonoBehaviour
    {
#if UNITY_EDITOR
        public float panSpeed = 25.0f;
        public float accelerationTime = 0.1f;
        public float sensitivity = 0.3f;
        public float fovMin = 5.0f;
        public float fovMax = 160.0f;

        [Range(0.0f, 1.0f)]
        public float fovCurrent = 0.5f;
        public float fovSens = 0.02f;
        
        [Range(1, 4)]
        public int resolutionScale = 1;

        private Vector3 moveTarget;
        private Vector3 velocity;
        private Vector2 cameraRotation;
        private Camera mainCamera;
        private bool screenshotTaken;
        private int cullingMask;
        private float lastScreenshotTime = float.MinValue;
        private string lastScreenshotName;

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
            
            transform.position = PhotoControllerPersistantData.GetPosition();
            transform.rotation = PhotoControllerPersistantData.GetRotation();
            fovCurrent = Mathf.InverseLerp(fovMin, fovMax, PhotoControllerPersistantData.GetFov());
            
            cameraRotation = new Vector2(transform.eulerAngles.y, -transform.eulerAngles.x);
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
            
            PhotoControllerPersistantData.SetPosition(transform.position);
            PhotoControllerPersistantData.SetRotation(transform.rotation);
            PhotoControllerPersistantData.SetFov(Mathf.Lerp(fovMin, fovMax, fovCurrent));
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

                lastScreenshotTime = Time.unscaledTime;
                lastScreenshotName = filename;
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

            var fovRad = Mathf.Lerp(fovMin, fovMax, fovCurrent) * Mathf.Deg2Rad;
            var sensitivity = this.sensitivity * Mathf.Tan(fovRad * 0.5f) * 2.0f;
            cameraRotation += Mouse.current.delta.ReadValue() * sensitivity;

            var kb = Keyboard.current;
            var panSpeed = this.panSpeed;

            if (kb.leftShiftKey.isPressed) panSpeed *= 2.0f;
            if (kb.leftCtrlKey.isPressed) panSpeed *= 2.0f;
            
            if (kb.leftAltKey.isPressed) panSpeed *= 0.5f;
            
            moveTarget = Vector3.ClampMagnitude(transform.TransformVector
            (
                kb.dKey.ReadValue() - kb.aKey.ReadValue(),
                kb.eKey.ReadValue() - kb.qKey.ReadValue(),
                kb.wKey.ReadValue() - kb.sKey.ReadValue()
            ), 1.0f) * panSpeed;

            fovCurrent += Mouse.current.scroll.ReadValue().y * 0.01f * fovSens;
            fovCurrent = Mathf.Clamp01(fovCurrent);
        }

        private void LateUpdate() { BindToCamera(); }

        private void BindToCamera()
        {
            mainCamera.transform.position = transform.position;
            mainCamera.transform.rotation = transform.rotation;
            mainCamera.fieldOfView = Mathf.Lerp(fovMin, fovMax, fovCurrent);
        }

        private void OnGUI()
        {
            var t = Time.unscaledTime - lastScreenshotTime;
            if (t < 5.0f)
            {
                GUI.Label(new Rect(15.0f, 15.0f, 500.0f, 100.0f), $"Screenshot Taken at \"{lastScreenshotName}\"");
            }

            GUI.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp01(1.0f - t * 5.0f));
            GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
        }

        private void OnValidate()
        {
            var mainCamera = Camera.main.transform;
            
            name = "PhotoController";
        }
#endif
    }
}