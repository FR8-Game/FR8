using FR8Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8Runtime.Rendering
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class SelectionOutline : MonoBehaviour
    {
        private void Update()
        {
            SelectionOutlinePass.Add(gameObject);
        }
    }
}
