using UnityEngine;

// ReSharper disable InconsistentNaming

namespace FR8Runtime
{
    public interface IBehaviour
    {
        GameObject gameObject { get; }
        Transform transform { get; }
    }
}