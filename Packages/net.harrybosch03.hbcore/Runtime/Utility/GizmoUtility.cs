using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HBCore.Utility
{
    public static class GizmoUtility
    {
        public static void Label(Vector3 position, string label)
        {
#if UNITY_EDITOR
            Handles.Label(position, label);
#endif
        }
    }
}