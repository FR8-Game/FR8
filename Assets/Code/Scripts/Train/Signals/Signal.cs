using System;
using UnityEngine;

namespace FR8.Runtime.Train.Signals
{
    [CreateAssetMenu(menuName = "Signal")]
    public class Signal : ScriptableObject
    {
        [SerializeField] private bool log;
        
        public event Action RaiseEvent;
        
        public static void Raise(Signal signal)
        {
            if (!signal) return;
            
            signal.RaiseEvent?.Invoke();
            signal.Log($"{signal.name} Raised at frame {Time.frameCount}");
        }

        // --- Logging ---

        private void Log(string text)
        {
            if (!log) return;
            Debug.Log(text, this);
        }
    }
}
