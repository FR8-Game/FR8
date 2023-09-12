using FR8Runtime.Contracts;
using UnityEditor;
using UnityEngine.UIElements;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(Contract))]
    public class ContractEditor : Editor<Contract>
    {
        public override void AddInspectorGUI(VisualElement root)
        {
            
        }
    }
}