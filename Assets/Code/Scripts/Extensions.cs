using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FR8.Train.Signals;
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
        
        private static bool SoftMatch(string a, string b)
        {
            string simplify(string str) => str.Trim().ToLower().Replace(" ", "");
            return simplify(a) == simplify(b);
        }
        
        public static T DeepFind<T>(this Component component, string name, Func<string, string, bool> match = null) where T : UnityEngine.Object
        {
            if (match == null) match = SoftMatch;

            foreach (var e in component.GetComponentsInChildren<T>())
            {
                if (match(e.name, name)) return e;
            }
            return default;
        }
        
        public static Transform DeepFind(this Transform transform, string name, Func<string, string, bool> match = null)
        {
            if (match == null) match = SoftMatch;
            
            var queue = new Queue<Transform>();
            queue.Enqueue(transform);

            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                if (match(t.name, name)) return t;

                foreach (Transform c in t)
                {
                    queue.Enqueue(c);
                }
            }

            return null;
        }
        
        public static Transform DeepFind(this Transform transform, Regex regex)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(transform);

            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                if (regex.IsMatch(t.name)) return t;

                foreach (Transform c in t)
                {
                    queue.Enqueue(c);
                }
            }

            return null;
        }

        public static void Listen(this List<Signal> signals, Action callback)
        {
            foreach (var signal in signals) signal.RaiseEvent += callback;
        }

        public static void Raise(this List<Signal> signals)
        {
            foreach (var signal in signals) Signal.Raise(signal);
        }

        public static T GetCachedComponent<T>(this Component component, ref T cache) where T : UnityEngine.Object
        {
            if (cache) return cache;
            return cache = component.GetComponent<T>();
        }
    }
}