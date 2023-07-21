using FR8.Drivers;
using UnityEngine;
using UnityEngine.Splines;

namespace FR8.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrainMovement : MonoBehaviour
    {
        [SerializeField] private float acceleration;
        [SerializeField] private float drag;
        [SerializeField] private float referenceWeight;

        [Space]
        [SerializeField] private SplineContainer track;
        [SerializeField] private int currentSplineIndex;

        [Space]
        [SerializeField] private DriverGroup throttleDriver;

        private new Rigidbody rigidbody;

        private float Throttle => throttleDriver ? throttleDriver.Value : 0.0f;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Move();
            ApplyDrag();
            ApplyCorrectiveForce();
        }


        private void Move()
        {
            rigidbody.AddForce(transform.forward * Throttle * acceleration * referenceWeight);
        }

        private void ApplyDrag()
        {
            var fwdSpeed = Vector3.Dot(transform.forward, rigidbody.velocity);
            var drag = -fwdSpeed * Mathf.Abs(fwdSpeed) * this.drag;

            rigidbody.AddForce(transform.forward * drag * referenceWeight);
        }

        private void ApplyCorrectiveForce()
        {
            var spline = track.Splines[currentSplineIndex];
            SplineUtility.GetNearestPoint(spline, rigidbody.position, out var nearest, out var t);
            nearest = track.transform.TransformPoint(nearest);

            var force = ((Vector3)nearest - rigidbody.position) / Time.deltaTime;
            
            Debug.DrawLine(rigidbody.position, nearest);

            var normalVelocity = rigidbody.velocity;
            normalVelocity -= transform.forward * Vector3.Dot(transform.forward, rigidbody.velocity);
            force -= normalVelocity;
            
            rigidbody.AddForce(force, ForceMode.VelocityChange);
            
            var tangent = spline.EvaluateTangent(t);
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            rigidbody = GetComponent<Rigidbody>();
            referenceWeight = rigidbody.mass;
        }
    }
}