
using UnityEngine;

namespace FR8.Runtime.Utility
{
    public sealed class HierarchyUnpacker
    {
        private Transform head;

        public HierarchyUnpacker(GameObject gameObject) : this(gameObject.transform) { }
        public HierarchyUnpacker(Component head)
        {
            this.head = head.transform;
        }

        public HierarchyUnpacker Next(int repeat)
        {
            for (var i = 0; i < repeat; i++) Next();
            return this;
        }
        
        public HierarchyUnpacker Next()
        {
            var isLastSibling = head.GetSiblingIndex() == head.parent.childCount - 1;
            
            if (isLastSibling) head = head.GetChild(0);
            else head = head.parent.GetChild(head.GetSiblingIndex() + 1);
            return this;
        }

        public HierarchyUnpacker Up()
        {
            head = head.parent;
            return this;
        }

        public Transform Get() => head;
        public T Get<T>() => head.GetComponent<T>();

        public static implicit operator Transform (HierarchyUnpacker unpacker) => unpacker.head;
    }
}