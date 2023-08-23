using UnityEngine;

namespace FR8Runtime.Interactions.Drivers.Submodules
{
    [System.Serializable]
    public class EnumSteppedDriver
    {
        [SerializeField] private Entry[] entries =
        {
            new("Off", 0.0f),
            new("On", 1.0f),
        };

        private int index;
        public string DisplayValue => entries[index].name;

        private int GetClosestEntry(float value)
        {
            var bestIndex = 0;
            for (var i = 1; i < entries.Length; i++)
            {
                var best = entries[bestIndex];
                var entry = entries[i];
                var bestDist = Mathf.Abs(value - best.value);
                var dist = Mathf.Abs(value - entry.value);
                if (dist > bestDist) continue;
                bestIndex = i;
            }
            return bestIndex;
        }

        public void Nudge(int direction)
        {
            var newIndex = index + direction;
            if (newIndex < 0 || newIndex >= entries.Length) return;
            ProcessValue(entries[newIndex].value);
        }

        public float ProcessValue(float newValue)
        {
            index = GetClosestEntry(newValue);
            return entries[index].value;
        }

        [System.Serializable]
        public class Entry
        {
            public string name;
            public float value;

            public Entry(string name, float value)
            {
                this.name = name;
                this.value = value;
            }
        }
    }
}