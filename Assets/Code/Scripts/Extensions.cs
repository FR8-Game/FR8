using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FMODUnity;
using FR8.Runtime.Train.Signals;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace FR8.Runtime
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

        public static bool State(this InputActionReference reference, float deadzone = 0.5f) => reference.action.State(deadzone);
        public static bool State(this InputAction action, float deadzone = 0.5f) => action.ReadValue<float>() > deadzone;
        
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

        public static void OneShot(this EventReference eventReference, GameObject gameObject)
        {
            if (eventReference.IsNull) return;
            
            var fmodEvent = RuntimeManager.CreateInstance(eventReference);
            fmodEvent.set3DAttributes(gameObject.To3DAttributes());
            fmodEvent.start();
            fmodEvent.release();
        }

        public static Color Alpha (this Color c, float a) => new Color(c.r, c.g, c.b, c.a * a);

        public static void SetTransform(this VisualEffect visualEffect, string propertyName, Transform transform)
        {
            visualEffect.SetTransform(propertyName, transform.position, transform.localRotation, transform.localScale);
        }

        public static bool HasTransform(this VisualEffect visualEffect, string propertyName)
        {
            const string vfxPositionPostfix = "_position";
            const string vfxRotationPostfix = "_angles";
            const string vfxScalePostfix = "_scale";
            
            var propPosition = propertyName + vfxPositionPostfix;
            var propEulerAngles = propertyName + vfxRotationPostfix;
            var propScale = propertyName + vfxScalePostfix;
 
            if (!visualEffect.HasVector3(propPosition)) return false;
            if (!visualEffect.HasVector3(propEulerAngles)) return false;
            if (!visualEffect.HasVector3(propScale)) return false;
            
            return true;
        }
        
        public static void SetTransform(this VisualEffect visualEffect, string propertyName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            const string vfxPositionPostfix = "_position";
            const string vfxRotationPostfix = "_angles";
            const string vfxScalePostfix = "_scale";
            
            var propPosition = propertyName + vfxPositionPostfix;
            var propEulerAngles = propertyName + vfxRotationPostfix;
            var propScale = propertyName + vfxScalePostfix;
 
            visualEffect.SetVector3(propPosition, position);
            visualEffect.SetVector3(propEulerAngles, rotation.eulerAngles);
            visualEffect.SetVector3(propScale, scale);
        }

        public static T Find<T>(this Transform transform, string path) => TryGetComponent<T>(transform.Find(path));
        public static T GetChild<T>(this Transform transform, int index) => TryGetComponent<T>(transform.GetChild(index));

        public static T Traverse<T>(this Transform transform, params int[] indices)
        {
            var head = transform;
            foreach (var i in indices)
            {
                if (i < 0 || i >= head.childCount)
                {
                    head = null;
                    break;
                }

                head = transform.GetChild(i);
            }
            return head ? head.GetComponent<T>() : default;
        }
        
        private static T TryGetComponent<T>(Transform transform)
        {
            return transform ? transform.GetComponent<T>() : default;
        }
    }
}