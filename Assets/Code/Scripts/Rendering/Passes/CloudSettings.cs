using UnityEngine;
using UnityEngine.Rendering;

namespace FR8.Runtime.Rendering.Passes
{
    [VolumeComponentMenu("Custom/Clouds")]
    public class CloudSettings : VolumeComponent
    {
        public VolumeParameter<Mesh> domeMesh = new();
        public VolumeParameter<Material> material = new();
    }
}