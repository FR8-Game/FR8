using FR8.Rendering.Passes;
using UnityEngine;

namespace FR8.Rendering
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class SelectionOutline : MonoBehaviour
    {
        private void OnEnable()
        {
            SelectionOutlinePass.RenderPersistant(gameObject);
        }

        private void OnDisable()
        {
            SelectionOutlinePass.RemovePersistant(gameObject);
        }
    }
}
