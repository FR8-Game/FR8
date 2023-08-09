
using System;
using UnityEngine;

namespace FR8.Rendering
{
    [SelectionBase, DisallowMultipleComponent]
    public class VolumetricLight : MonoBehaviour
    {
        [SerializeField] private float density;

        private Light light;

        public Light Light
        {
            get
            {
                if (!light)
                {
                    light = GetComponent<Light>();
                }
                return light;
            }
        }
        public float Density => density;
    }
}