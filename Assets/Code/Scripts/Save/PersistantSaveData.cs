using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FR8Runtime.Save
{
    [Serializable]
    public class PersistantSaveData
    {
        // Graphics
        public int xResolution = 1280;
        public int yResolution = 720;
        public int refreshRate = 60;
        public int quality = (int)GraphicsTier.Tier1;
        public int displayMode = (int)FullScreenMode.Windowed;
        
        // Audio
        public float masterVolume = 1.0f;
        public float sfxVolume = 1.0f;
        public float musicVolume = 1.0f;
        
        // Accessibility
        public float fieldOfView = 70.0f;
        public float whiteNoiseVolume = 1.0f;
        public float mouseSensitivity = 0.3f;
        public float gamepadSensitivityX = 0.3f;
        public float gamepadSensitivityY = 0.3f;
    }
}