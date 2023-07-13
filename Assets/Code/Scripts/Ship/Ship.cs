using FR8.Interactions;
using UnityEngine;

namespace FR8.Ship
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Ship : MonoBehaviour
    {
        [SerializeField] private float referenceWeight;
        [SerializeField] private Vector3 thrustAcceleration;
        [SerializeField] private float turnAcceleration;

        [Space] 
        [SerializeField] private DriverGroup linearThrottle;
        [SerializeField] private DriverGroup angularThrottle;

        private Vector3 linearThrust;
        private Vector3 angularThrust;
        
        public Rigidbody Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }
        
        private void FixedUpdate()
        {
            linearThrust = linearThrottle.Composite3();
            angularThrust = angularThrottle.Composite3();
            
            ApplyThrust();
        }

        private void ApplyThrust()
        {
            var force = transform.TransformDirection(Vector3.Scale(linearThrust, thrustAcceleration)) * referenceWeight;
            Rigidbody.AddForce(force);

            var torque = transform.TransformDirection(angularThrust * turnAcceleration) * referenceWeight;
            Rigidbody.AddTorque(torque);
        }
    }
}
