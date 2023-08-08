using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerZeroGMovement : PlayerAvatar
    {
        [SerializeField] private float maxSpeed = 12.0f;
        [SerializeField] private float accelerationTime = 1.5f;
        [SerializeField] private float rotationSpring = 2.0f;
        [SerializeField] private float rotationDamping = 0.4f;
        [SerializeField] private float collisionRadius = 0.2f;

        private new Camera camera;
        private new SphereCollider collider;
        
        private Quaternion cameraOrientation = Quaternion.identity;
        
        protected override void Awake()
        {
            base.Awake();
            camera = Camera.main;
            
            Configure();
        }

        private void OnEnable()
        {
            cameraOrientation = camera.transform.rotation;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnValidate()
        {
            Configure();
        }

        protected override void Configure()
        {
            base.Configure();
            
            collider = gameObject.GetOrAddComponent<SphereCollider>();
            collider.radius = collisionRadius;
        }

        private void FixedUpdate()
        {   
            Move();
            Rotate();
        }

        private void Update()
        {
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            var lookDelta = Controller.LookFrameDelta;
            cameraOrientation *= Quaternion.Euler(-lookDelta.y, lookDelta.x, 0.0f);

            camera.transform.rotation = cameraOrientation;
            camera.transform.position = transform.position;
        }

        private void Move()
        {
            var input = Controller.Move;
            var target = camera.transform.TransformDirection(input) * maxSpeed;
            var current = Rigidbody.velocity;

            var force = Vector3.ClampMagnitude(target - current, maxSpeed) / accelerationTime;
            Rigidbody.AddForce(force, ForceMode.Acceleration);
        }
        
        private void Rotate()
        {
            (cameraOrientation * Quaternion.Inverse(transform.rotation)).ToAngleAxis(out var angle, out var axis);

            var torque = axis * angle * rotationSpring - Rigidbody.angularVelocity * rotationDamping;
            Rigidbody.AddTorque(torque, ForceMode.Acceleration);
        }
    }
}