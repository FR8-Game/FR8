using System;
using FR8Runtime.Contracts;
using FR8Runtime.Train.Track;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace FR8Runtime.Testing
{
    public class TestingKart : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private bool reset;

        private TrackSegment segment;
        private Rigidbody body;

        private float positionOnSpline;
        private float lastPositionOnSpline;
        private Vector3 startPosition;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation;

            startPosition = transform.position;

            float t;
            (segment, t) = TrackUtility.FindClosestSegment(transform.position);
            body.position = segment.SamplePoint(t);
        }

        private void FixedUpdate()
        {
            segment = segment.CheckForJunctions(transform.position, positionOnSpline, lastPositionOnSpline, segment);
            
            lastPositionOnSpline = positionOnSpline;
            positionOnSpline = segment.GetClosestPoint(transform.position, true);

            ApplyConstraintForce();
            ApplyDriveForce();

            if (reset)
            {
                reset = false;
                
                transform.position = startPosition;
                speed = 0.0f;

                float t;
                (segment, t) = TrackUtility.FindClosestSegment(transform.position);
                body.position = segment.SamplePoint(t);
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
            var point = segment.SamplePoint(positionOnSpline);
            var tangent = segment.SampleTangent(positionOnSpline);

            var force = (point - body.position) / Time.deltaTime;
            force -= body.velocity;
            force -= tangent * Vector3.Dot(tangent, force);
            
            body.AddForce(force, ForceMode.VelocityChange);
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        }
    }
}
