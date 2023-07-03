using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerNoClip : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 12.0f;
        [SerializeField] private float accelerationTime = 0.2f;
        
        private new Rigidbody rigidbody;

        public Vector3 MoveInput { get; set; }
        
        private void OnEnable()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.detectCollisions = false;
            rigidbody.useGravity = false;
        }

        private void FixedUpdate()
        {
            var target = transform.TransformDirection(MoveInput) * moveSpeed;
            var current = rigidbody.velocity;

            var difference = target - current;
            var force = Vector3.ClampMagnitude(difference, moveSpeed) / accelerationTime;
            rigidbody.AddForce(force, ForceMode.Acceleration);
        }
    }
}
