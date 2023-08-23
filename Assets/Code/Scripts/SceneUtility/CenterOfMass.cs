using UnityEngine;

namespace FR8Runtime.SceneUtility
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