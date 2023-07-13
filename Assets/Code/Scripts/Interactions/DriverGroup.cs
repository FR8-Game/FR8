using UnityEngine;

namespace FR8.Interactions
{
    [SelectionBase, DisallowMultipleComponent]
    public class DriverGroup : MonoBehaviour
    {
        private LinearDriver[] drivers;

        private void Awake()
        {
            drivers = new LinearDriver[transform.childCount];
            for (var i = 0; i < transform.childCount; i++)
            {
                drivers[i] = transform.GetChild(i).GetComponent<LinearDriver>();
            }
        }

        public float Channel(int channel) => channel >= 0 && channel < drivers.Length ? drivers[channel].Output : 0.0f;
        public Vector2 Composite2() => new(Channel(0), Channel(1));
        public Vector3 Composite3() => new(Channel(0), Channel(1), Channel(2));

        private void Reset()
        {
            gameObject.name = "Driver Group";
            
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child.GetComponent<LinearDriver>()) continue;

                var append = "[" + i switch
                {
                    0 => "X",
                    1 => "Y",
                    2 => "Z",
                    _ => $"{i}",
                } + "] ";

                if (child.name.Contains('[') && child.name.Contains(']'))
                {
                    var end = child.name.IndexOf(']') + 2;
                    child.name = append + child.name[end..];
                }
                else
                {
                    child.name = append + child.name;
                }
            }
        }
    }
}