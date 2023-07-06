using System;
using UnityEngine;

namespace TerrainHandles
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

        public static void ComponentWise(this Vector2 vector, Action<float, Vector2> callback)
        {
            callback(vector.x, Vector3.right);
            callback(vector.y, Vector3.up);
        }
        
        public static void ComponentWise(this Vector3 vector, Action<float, Vector3> callback)
        {
            callback(vector.x, Vector3.right);
            callback(vector.y, Vector3.up);
            callback(vector.z, Vector3.forward);
        }
    }
}