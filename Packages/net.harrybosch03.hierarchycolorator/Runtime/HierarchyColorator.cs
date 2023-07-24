using UnityEngine;

namespace HierarchyColorator
{
    public partial class HierarchyColorator : MonoBehaviour
    {
        [SerializeField] private Color overlay = new(1.0f, 1.0f, 1.0f, 0.0f);
        [SerializeField] private Color sidebar = new(1.0f, 1.0f, 1.0f, 0.0f);
    }
}