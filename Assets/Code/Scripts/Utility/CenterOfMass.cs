using UnityEngine;

namespace FR8
{
    public class CenterOfMass : MonoBehaviour
    {
        private void Awake()
        {
            var rb = GetComponentInParent<Rigidbody>();
            rb.centerOfMass = rb.transform.InverseTransformPoint(transform.position);
        }
    }
}