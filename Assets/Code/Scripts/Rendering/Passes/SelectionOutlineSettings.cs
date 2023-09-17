using UnityEngine;
using UnityEngine.Rendering;

namespace FR8Runtime.Rendering.Passes
{
    [VolumeComponentMenu("Custom/Selection Outline")]
    public class SelectionOutlineSettings : VolumeComponent
    {
        public ColorParameter outlineColor = new(Color.white);
    }
}