using UnityEngine;

namespace FR8.Drivers
{
    public class EnumSteppedDriver : Driver
    {
        [SerializeField] private Entry[] entries =
        {
            new("Off", 0.0f),
            new("On", 1.0f),
        };

        private int index;
        public override string DisplayValue => entries[index].name;

        public override float Value
        {
            get => base.Value;
            set
            {
                index = GetClosestEntry(value);
                base.Value = entries[index].value;
            }
        }

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

        public override void Nudge(int direction)
        {
            var newIndex = index + direction;
            if (newIndex < 0 || newIndex >= entries.Length) return;
            Value = entries[newIndex].value;
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