using UnityEngine;

namespace FR8.Utility
{
    public class Hierarchy
    {
        public static Transform FindOrCreate(Transform parent, string name)
        {
            var group = parent.Find(name);
            return group ? group : NewChild(parent, name);
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
            newChild.localRotation = Quaternion.identity;
            newChild.localScale = Vector3.one;
            return newChild;
        }
    }
}