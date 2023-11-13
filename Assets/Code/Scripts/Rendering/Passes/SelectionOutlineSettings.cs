using UnityEngine;
using UnityEngine.Rendering;

namespace FR8.Runtime.Rendering.Passes
{
    [VolumeComponentMenu("Custom/Selection Outline")]
    public class SelectionOutlineSettings : VolumeComponent
    {
        public ColorParameter outlineColor = new(Color.white);
    }
}