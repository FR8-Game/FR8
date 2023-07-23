using UnityEngine;
using UnityEngine.Serialization;

namespace FR8.Drivers
{
    public class EnumSteppedDriver : Driver
    {
        [SerializeField] private Entry[] entries =
        {
            new("Off", 0.0f),
            new("On", 1.0f),
        };

        public override string DisplayValue => entries[Index].name;
        public int Index { get; private set; }

        protected override void Start()
        {
            base.Start();
            SetIndex(Mathf.RoundToInt(Value));
        }

        public override void Nudge(int direction)
        {
            SetIndex(Index + direction);
        }

        public override void Press()
        {
            SetIndex(Index++);
        }

        public void SetIndex(int newIndex)
        {
            Index = newIndex;
            Index = Mathf.Clamp(Index, 0, entries.Length - 1);
            Value = entries[Index].value;
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