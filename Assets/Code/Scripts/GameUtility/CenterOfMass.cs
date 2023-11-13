using UnityEngine;

namespace FR8.Runtime.GameUtility
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