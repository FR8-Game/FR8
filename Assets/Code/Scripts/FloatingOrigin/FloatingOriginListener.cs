using UnityEngine;

namespace FR8.FloatingOrigin
{
    [SelectionBase, DisallowMultipleComponent]
    public class FloatingOriginListener : MonoBehaviour
    {
        private void OnEnable()
        {
            FloatingOriginDriver.offsetEvent += OnOffset;
        }

        private void OnDisable()
        {
            FloatingOriginDriver.offsetEvent -= OnOffset;
        }

        protected virtual void OnOffset(Vector3 offset)
        {
            transform.position += offset;
        }
    }
}