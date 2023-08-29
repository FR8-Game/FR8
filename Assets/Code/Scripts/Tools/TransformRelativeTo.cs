using UnityEngine;

namespace FR8Runtime.Tools
{
    [ExecuteAlways]
    public sealed class TransformRelativeTo : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private Vector3 localPosition;
        [SerializeField] private Vector3 localRotation;

        public bool Locked { get; set; }
        
        private void LateUpdate()
        {
            if (Locked) UpdateTransform();
            else UpdateSelf();
        }

        private void OnValidate()
        {
            if (Locked) UpdateTransform();
            else UpdateSelf();
        }

        public void UpdateTransform()
        {
            var other = GetOther();

            transform.position = other.TransformPoint(localPosition);
            transform.rotation = other.rotation * Quaternion.Euler(localRotation);
        }

        public void UpdateSelf()
        {
            var other = GetOther();

            localPosition = other.InverseTransformPoint(transform.position);
            localRotation = (Quaternion.Inverse(other.rotation) * transform.rotation).eulerAngles;
        }

        public Transform GetOther()
        {
            if (parent) return parent;
            if (transform.parent) return transform.parent;
            return transform;
        }
    }
}
