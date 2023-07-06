
using UnityEngine;

namespace FR8.FloatingOrigin
{
    [SelectionBase, DisallowMultipleComponent]
    public class FloatingOriginListenerGroup : FloatingOriginListener
    {
        protected override void OnOffset(Vector3 offset)
        {
            foreach (Transform child in transform)
            {
                child.position += offset;
            }
        }
    }
}