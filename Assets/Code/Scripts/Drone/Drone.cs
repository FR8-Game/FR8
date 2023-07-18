using FR8.Player;
using UnityEngine;

namespace FR8.Drone
{
    public class Drone : PlayerAvatar
    {
        [SerializeField] private float moveSpeed = 10.0f;
        [SerializeField] private float accelerationTime = 1.0f;
        [SerializeField] private float turnSpring;
        [SerializeField] private float turnDamping;
        
        private Rigidbody referenceTarget;
        private new Camera camera;

        private Transform camTarget;
        private Quaternion orientation = Quaternion.identity;
        
        public Vector3 LocalVelocity => Rigidbody.velocity - (referenceTarget ? referenceTarget.velocity : Vector3.zero);

        protected override void Awake()
        {
            base.Awake();
            camera = Camera.main;

            camTarget = transform.Find("Cam Target");
        }


        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void FixedUpdate()
        {
            Move();
            Turn();
        }

        private void Move()
        {
            var target = transform.TransformDirection(Controller.Move) * moveSpeed;
            var force = Vector3.ClampMagnitude(target - LocalVelocity, moveSpeed) / accelerationTime;
            Rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Turn()
        {
            (orientation * Quaternion.Inverse(Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);

            var torque = axis * angle * turnSpring - Rigidbody.angularVelocity * turnDamping;
            Rigidbody.AddTorque(torque, ForceMode.Acceleration);
        }

        private void LateUpdate()
        {
            camera.transform.position = camTarget.position;
            camera.transform.rotation = orientation;

            var delta = Controller.LookFrameDelta;
            orientation *= Quaternion.Euler(-delta.y, delta.x, delta.z);
        }
    }
}
