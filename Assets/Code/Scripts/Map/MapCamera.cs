using System;
using UnityEngine;

namespace FR8.Map
{
    [RequireComponent(typeof(Camera))]
    public class MapCamera : MonoBehaviour
    {
        [SerializeField] private Material material;

        private void OnValidate()
        {
            var camera = GetComponent<Camera>();
            camera.
        }
    }
}
