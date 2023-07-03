using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8
{
    public static class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject, Action<T> addCallback = null) where T : Component
        {
            if (gameObject.TryGetComponent(out T res)) return res;

            res = gameObject.AddComponent<T>();
            addCallback?.Invoke(res);
            return res;
        }

        public static bool Switch(this InputActionReference reference, float deadzone = 0.5f) => reference.action.Switch(deadzone);
        public static bool Switch(this InputAction action, float deadzone = 0.5f) => action.ReadValue<float>() > deadzone;
    }
}