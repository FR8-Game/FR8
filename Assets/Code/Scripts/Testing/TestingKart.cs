using System;
using FR8Runtime.Train.Track;
using UnityEngine;

namespace FR8Runtime.Testing
{
    [RequireComponent(typeof(Rigidbody))]
    public class TestingKart : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private bool reset;

        private Rigidbody body;
        private TrackRunner runner;

        private Vector3 startPosition;
        private Vector3 splinePoint;
        private Vector3 splineVelocity;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation;

            startPosition = transform.position;

            runner = new TrackRunner(transform.position);
        }

        private void FixedUpdate()
        {
            runner.FixedUpdate(body.position);
            ApplyConstraintForce();
            ApplyDriveForce();

            if (reset)
            {
                reset = false;
                
                transform.position = startPosition;
                speed = 0.0f;

                float t;
                runner = new TrackRunner(transform.position);
            }
        }

        private void ApplyDriveForce()
        {
            var force = transform.forward * speed;
            force -= transform.forward * Vector3.Dot(transform.forward, body.velocity);
            body.AddForce(force, ForceMode.VelocityChange);
        }

        private void ApplyConstraintForce()
        {
            splinePoint = runner.Position;
            splineVelocity = runner.Direction;

            var force = (splinePoint - body.position) / Time.deltaTime;
            force -= body.velocity;
            force -= splineVelocity * Vector3.Dot(splineVelocity, force);
            
            body.AddForce(force, ForceMode.VelocityChange);
            transform.rotation = Quaternion.LookRotation(splineVelocity, Vector3.up);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, splinePoint);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, splineVelocity);
        }
    }
}
