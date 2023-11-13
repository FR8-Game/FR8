using UnityEngine;

// ReSharper disable InconsistentNaming

namespace FR8.Runtime
{
    public interface IBehaviour
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        bool isActiveAndEnabled { get; }
    }
}