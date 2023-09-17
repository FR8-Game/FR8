using System;
using UnityEngine;

namespace FR8Runtime.UI
{
    public static class Pause
    {   
        public static bool Paused { get; private set; }
        public static event Action PausedStateChangedEvent;

        public static void TogglePaused() => SetPaused(!Paused);
        public static void SetPaused(bool state)
        {
            if (Paused == state) return;

            Paused = state;
            Time.timeScale = state ? 0.0f : 1.0f;

            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Paused", state ? 1.0f : 0.0f);

            PausedStateChangedEvent?.Invoke();
        }
    }
}