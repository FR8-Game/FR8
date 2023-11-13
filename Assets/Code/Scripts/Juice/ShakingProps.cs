using UnityEngine;
using Random = UnityEngine.Random;

namespace FR8.Runtime.Juice
{
    public class ShakingProps : MonoBehaviour
    {
        public float amplitude = 1.5f;
        public float maxSpeed = 100.0f;
        [Range(0.0f, 1.0f)] public float smoothing = 0.75f;
        public AnimationCurve remapCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [Space] 
        public Transform visuals;
        public Vector3 axisA = Vector3.right;
        public Vector3 axisB = Vector3.forward;

        private Vector2 shake;
        private Vector3 lastPosition;

        private void Awake()
        {
            if (!visuals) visuals = transform.GetChild(0);
        }

        private void FixedUpdate()
        {
            var delta = transform.position - lastPosition;
            lastPosition = transform.position;
            var speed = delta.magnitude / Time.deltaTime;
         
            var sample = Random.insideUnitCircle;
            shake = Vector2.Lerp(sample, shake, smoothing);
            
            var rotation = shake.normalized * remapCurve.Evaluate(shake.magnitude) * amplitude * (speed / maxSpeed);
            visuals.localRotation = Quaternion.Euler(axisA * rotation.x + axisB * rotation.y);
        }
    }
}
