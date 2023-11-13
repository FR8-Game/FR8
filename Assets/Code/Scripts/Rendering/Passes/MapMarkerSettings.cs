using UnityEngine.Rendering;

namespace FR8.Runtime.Rendering.Passes
{
    [VolumeComponentMenu("Custom/Map Markers")]
    public class MapMarkerSettings : VolumeComponent
    {
        public FloatParameter markerScale = new(1.0f);
    }
}