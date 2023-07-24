#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HierarchyColorator
{
    [InitializeOnLoad]
    public partial class HierarchyColorator
    {
        static HierarchyColorator()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        public static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (!obj) return;
            
            var tint = obj.GetComponent<HierarchyColorator>();

            if (!tint) return;
            
            var color = tint.sidebar;

            var sidebarRect = new Rect(selectionRect.x + 60.0f - 28.0f - selectionRect.xMin, selectionRect.position.y, 2.0f, selectionRect.size.y);
            EditorGUI.DrawRect(sidebarRect, color);

            color = tint.overlay;
            color.a *= 0.2f;

            var backgroundRect = new Rect(selectionRect.x + 60.0f - 28.0f - selectionRect.xMin, selectionRect.position.y, selectionRect.size.x + selectionRect.xMin + 28.0f - 60.0f + 16.0f, selectionRect.size.y);
            EditorGUI.DrawRect(backgroundRect, color);
        }

        private void OnValidate()
        {
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
#endif