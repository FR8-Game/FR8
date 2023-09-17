using System;
using UnityEngine;

namespace FR8Runtime.Save
{
    [Serializable]
    public class PersistantSaveData
    {
        public string lastSaveName;
        public float playerAvatarFov = 70.0f;
        public int xResolution = 1280;
        public int yResolution = 720;
        public int fullscreenMode = (int)FullScreenMode.Windowed;
    }
}