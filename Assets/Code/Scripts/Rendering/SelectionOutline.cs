using System.Collections.Generic;
using FR8.Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8.Runtime.Rendering
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class SelectionOutline : MonoBehaviour
    {
        private IEnumerable<Renderer> renderers;

        private void OnEnable()
        {
            renderers = GetComponentsInChildren<Renderer>();
            SelectionOutlinePass.Add(renderers);
        }

        private void OnDisable()
        {
            SelectionOutlinePass.Remove(renderers);
        }
    }
}
