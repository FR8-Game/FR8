using System;
using System.Collections.Generic;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class DriverNetwork : MonoBehaviour
    {
        private Dictionary<string, float> values = new();

        public event Action<string, float> ValueChangedEvent;
        
        public void SetValue(string key, float value)
        {
            key = Simplify(key);
            
            if (values.ContainsKey(key)) values[key] = value;
            else values.Add(key, value);
            
            ValueChangedEvent?.Invoke(key, value);
        }

        public static bool CompareKeys(string a, string b) => Simplify(a) == Simplify(b);
        public static string Simplify(string str) => str.Trim().ToLower().Replace(" ", "");

        public float GetValue(string key, float fallback = default)
        {
            key = Simplify(key);
            return values.ContainsKey(key) ? values[key] : fallback;
        }

        public IEnumerable<KeyValuePair<string, float>> GetEnumerator() => values;
    }
}