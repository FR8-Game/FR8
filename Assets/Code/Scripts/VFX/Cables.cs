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
            position = Vector3.zero;
            velocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            var targetVelocity = body ? body.GetPointVelocity(transform.position) : Vector3.zero;
            var force = -position * spring - velocity * damper;

            velocity += Random.insideUnitSphere * targetVelocity.magnitude * velocityShake * Time.deltaTime;

            position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            
            propertyBlock.SetVector("_Offset", position * response);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
