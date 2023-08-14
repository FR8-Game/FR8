using System.Text.RegularExpressions;
using UnityEngine;

namespace FR8
{
    public static partial class Utility
    {
        public class Hierarchy
        {
            public static Transform FindOrCreate(Transform parent, string name)
            {
                return FindOrCreate(parent, new Regex($".+{name}.+"), name);
            }

            public static Transform FindOrCreate(Transform parent, Regex pattern, string newName)
            {
                foreach (Transform child in parent)
                {
                    if (pattern.IsMatch(child.name)) return child;
                }

                return NewChild(parent, newName);
            }

            public static Transform GetOrCreateChildren(Transform parent, string name, int index)
            {
                for (var i = 0; i < index + 1 - parent.childCount; i++)
                {
                    NewChild(parent, "New Child");
                }

                var child = parent.GetChild(index);
                child.name = name;
                return child;
            }

            public static Transform NewChild(Transform parent, string name)
            {
                var newChild = new GameObject(name).transform;
                newChild.SetParent(parent);
                newChild.localPosition = Vector3.zero;
                newChild.localRotation = UnityEngine.Quaternion.identity;
                newChild.localScale = Vector3.one;
                return newChild;
            }
        }
    }
}