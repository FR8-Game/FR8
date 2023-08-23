
using UnityEngine;

namespace FR8Runtime.CodeUtility
{
    public static class FindUtility
    {
        public static T Find<T>(Transform root, string path, bool log = true)
        {
            var find = root.Find(path);
            if (!find)
            {
                if (log) Debug.LogError($"Could not find \"{path}\" on {root.name}", root);
                return default;
            }
            
            return find.GetComponent<T>();
        }
    }
}