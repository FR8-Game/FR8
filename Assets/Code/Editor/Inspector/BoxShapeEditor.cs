using FR8.Runtime.Shapes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(BoxShape))]
    public sealed class BoxShapeEditor : Editor
    {
        private void OnSceneGUI()
        {
            var box = target as BoxShape;
            
            Handles.matrix = box.transform.localToWorldMatrix;
            
            Handles.color = ColorReference.Success;
            Handles.zTest = CompareFunction.Less;
            Handles.DrawWireCube(box.center, box.size);   
            
            Handles.color = ColorReference.Failure;
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawWireCube(box.center, box.size);
            
            Handles.color = Color.white;
            Handles.zTest = CompareFunction.Equal;
            Handles.DrawWireCube(box.center, box.size);   
        }
    }
}
