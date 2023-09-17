using UnityEngine;
using UnityEngine.Rendering;

namespace FR8Runtime.Rendering.Passes
{
    [VolumeComponentMenu("Custom/Fog")]
    public class FogSettings : VolumeComponent
    {
        public ColorParameter color = new(new Color(1.0f, 1.0f, 1.0f, 0.1f));
        public ClampedFloatParameter density = new(0.5f, 0.0f, 1.0f);
        public FloatParameter heightFalloffLower = new(5.0f);
        public FloatParameter heightFalloffUpper = new(30.0f);
        public BoolParameter renderOverSkybox = new(false);
        public BoolParameter showFogInSceneView = new(false);
    }
}