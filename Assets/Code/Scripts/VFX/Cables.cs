using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FR8.Runtime.VFX
{
    public class Cables : MonoBehaviour
    {
        public float spring = 300.0f;
        public float damper = 25.0f;
        public float response = 0.5f;
        public float velocityShake;
        
        private new MeshRenderer renderer;
        private MaterialPropertyBlock propertyBlock;
        private Rigidbody body;

        private Vector3 position;
        private Vector3 velocity;
        
        private void Awake()
        {
            body = GetComponentInParent<Rigidbody>();
            renderer = GetComponent<MeshRenderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            position = transform.position;
        }

        private void FixedUpdate()
        {
            var targetVelocity = body ? body.velocity : Vector3.zero;
            var force = (transform.position - position) * spring + (targetVelocity - velocity) * damper;

            velocity += Random.insideUnitSphere * targetVelocity.magnitude * velocityShake * Time.deltaTime;

            position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            
            propertyBlock.SetVector("_Offset", (position - transform.position) * response);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
