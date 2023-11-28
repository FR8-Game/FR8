#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace FR8.Runtime.Player
{
    [InitializeOnLoad]
    public static class PhotoControllerPersistantData
    {
        static PhotoControllerPersistantData() { EditorApplication.playModeStateChanged += OnPlaymodeStateChanged; }

        private static void OnPlaymodeStateChanged(PlayModeStateChange args)
        {
            if (args != PlayModeStateChange.EnteredEditMode) return;
            
            var mainCamera = Camera.main;
            var position = GetPosition();
            var rotation = GetRotation();

            mainCamera.transform.position = position;
            mainCamera.transform.rotation = rotation;

            foreach (var e in Object.FindObjectsOfType<PhotoController>())
            {
                e.transform.position = position;
                e.transform.rotation = rotation;
            }
        }

        private static Vector3 Get(string key, Vector3 fallback) => new()
        {
            x = EditorPrefs.GetFloat($"PhotoControllerPersistantData.{key}.x", fallback.x),
            y = EditorPrefs.GetFloat($"PhotoControllerPersistantData.{key}.y", fallback.y),
            z = EditorPrefs.GetFloat($"PhotoControllerPersistantData.{key}.z", fallback.z),
        };

        private static void Set(string key, Vector3 value)
        {
            EditorPrefs.SetFloat($"PhotoControllerPersistantData.{key}.x", value.x);
            EditorPrefs.SetFloat($"PhotoControllerPersistantData.{key}.y", value.y);
            EditorPrefs.SetFloat($"PhotoControllerPersistantData.{key}.z", value.z);
        }

        public static Vector3 GetPosition() => Get("position", Camera.main.transform.position);
        public static void SetPosition(Vector3 value) => Set("position", value);

        public static Quaternion GetRotation() => Quaternion.Euler(Get("rotation", Camera.main.transform.eulerAngles));
        public static void SetRotation(Quaternion rotation) => Set("rotation", rotation.eulerAngles);

        public static float GetFov() => EditorPrefs.GetFloat("PhotoControllerPersistantData.fov", Camera.main.fieldOfView);
        public static void SetFov(float value) => EditorPrefs.SetFloat("PhotoControllerPersistantData.fov", value);
    }
}
#endif