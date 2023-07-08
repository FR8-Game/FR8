using UnityEngine;
// ReSharper disable InconsistentNaming

namespace FR8
{
    public interface IBehaviour
    {
        GameObject gameObject { get; }
        Transform transform { get; }
    }
}