using System;
using FR8.Rendering.Passes;
using FR8Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8.Rendering
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
