using UnityEngine;

namespace FR8Runtime.GameUtility
{
    public class CenterOfMass : MonoBehaviour
    {
        private void Start()
        {
            var rb = GetComponentInParent<Rigidbody>();
            rb.centerOfMass = rb.transform.InverseTransformPoint(transform.position);
        }
    }
}