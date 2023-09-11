using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FR8Runtime.Shapes
{
    public abstract class Shape : MonoBehaviour
    {
        public abstract bool ContainsPoint(Vector3 point);
        public abstract void Draw(Action<Vector3, Vector3> drawLine);

        protected virtual void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            Handles.color = ColorReference.Primary;
            Handles.matrix = transform.localToWorldMatrix;
            Draw((a, b) => Handles.DrawAAPolyLine(a, b));
#endif
        }

        public void DrawGizmos() => OnDrawGizmosSelected();

        public static void FindShapes(List<Shape> list, Transform root, string path, bool clear = true, bool createGroupIfMissing = false)
        {
            var container = string.IsNullOrEmpty(path) ? root : root.Find(path);
            if (!container)
            {
                if (!createGroupIfMissing) return;
                if (string.IsNullOrEmpty(path)) return;

                var names = path.Split('/');
                var head = root;
                foreach (var name in names)
                {
                    var next = head.Find(name);
                    if (!next)
                    {
                        next = new GameObject(name).transform;
                        next.SetParent(head);
                        next.localPosition = Vector3.zero;
                        next.localRotation = Quaternion.identity;
                    }

                    head = next;
                }
                container = head;
            }

            if (clear) list.Clear();
            list.AddRange(container.GetComponentsInChildren<Shape>());
        }

        public enum TagOperation
        {
            Whitelist,
            Blacklist
        }
    }
}