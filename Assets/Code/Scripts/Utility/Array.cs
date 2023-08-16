using System.Collections.Generic;

namespace FR8
{
    public static partial class Utility
    {
        public static class Array
        {
            public static IndexOperation<T> IndexFallback<T>(T fallback) => (array, index) => IndexFallback(array, index, fallback);

            public static T IndexFallback<T>(T[] array, int index, T fallback = default)
            {
                if (index < 0 || index >= array.Length) return fallback;
                return array[index];
            }

            public static T IndexLoop<T>(T[] array, int index)
            {
                index = (index % array.Length + array.Length) % array.Length;
                return array[index];
            }

            public static T IndexLoop<T>(List<T> array, int index)
            {
                index = (index % array.Count + array.Count) % array.Count;
                return array[index];
            }

            public delegate T IndexOperation<T>(T[] array, int index);
        }
    }
}