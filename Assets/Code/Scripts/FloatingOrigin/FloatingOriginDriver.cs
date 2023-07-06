using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FR8.FloatingOrigin
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class FloatingOriginDriver : FloatingOriginListener
    {
        [SerializeField] private float range = 50.0f;

        public static Action<Vector3> offsetEvent;

        private void FixedUpdate()
        {
            if (transform.position.sqrMagnitude > range * range)
            {
                offsetEvent?.Invoke(-transform.position);
            }
        }
    }
}