using System;
using System.Collections.Generic;
using FR8Runtime.Train.Track;
using UnityEngine;

namespace FR8Runtime.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class TrainCarriage : MonoBehaviour, INameplateProvider
    {
        [Space]
        public string saveTypeReference = "TrainCarriage";
        
        [Space]
        [SerializeField] protected float drag = 12.0f;
        [SerializeField] protected float referenceWeight;
        [SerializeField] protected float cornerLean = 0.6f;

        [Space]
        [SerializeField] protected float retentionSpring = 2500.0f;
        [SerializeField] protected float retentionDamping = 50.0f;
        [SerializeField] protected float retentionTorqueConstant = 0.2f;
        
        [SerializeField] protected float trainLength = 20.0f;

        protected TrackSegment segment;

        private float softAnchorPositionOnSpline;

        public TrackSegment Segment
        {
            get => segment;
            set => segment = value;
        }

        public string Name => name;
        public Rigidbody Rigidbody { get; private set; }
        public Vector3 DriverDirection { get; private set; }
        public float ReferenceWeight => referenceWeight;
        public Vector3 TangentialForce { get; private set; }
        public float PositionOnSpline { get; private set; }
        public float LastPositionOnSpline { get; private set; }

        public Vector3 HardAnchorPosition => (Rigidbody ? Rigidbody.position : transform.position) + transform.forward * trainLength * 0.5f;
        public Vector3 SoftAnchorPosition => (Rigidbody ? Rigidbody.position : transform.position) - transform.forward * trainLength * 0.5f;
        
        public static readonly List<TrainCarriage> All = new();
        
        private void Awake()
        {
            Configure();
        }

        private void OnEnable()
        {
            All.Add(this);
            FindClosestSegment();
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        public void FindClosestSegment()
        {
            var best = (TrackSegment)null;
            var bestScore = float.MaxValue;
            var point = transform.position;
            var tangent = transform.forward;
            
            var segments = FindObjectsOfType<TrackSegment>();
            foreach (var segment in segments)
            {
                var t = segment.GetClosestPoint(transform.position);
                var closestPoint = segment.SamplePoint(t);
                var score = (closestPoint - transform.position).sqrMagnitude;

                if (score > bestScore) continue;

                bestScore = score;
                best = segment;
                point = closestPoint;
                tangent = segment.SampleTangent(t);
            }

            segment = best;
            transform.position = point;
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        }

        protected virtual void Configure()
        {
            Rigidbody = GetComponent<Rigidbody>();
            if (!Application.isPlaying)
            {
                referenceWeight = Rigidbody.mass;
            }
        }

        protected virtual void FixedUpdate()
        {
            LastPositionOnSpline = PositionOnSpline;
            PositionOnSpline = segment.GetClosestPoint(HardAnchorPosition);
            softAnchorPositionOnSpline = segment.GetClosestPoint(SoftAnchorPosition);

            ApplyDrag();
            ApplyCorrectiveForce();
            
            if(segment) segment.UpdateConnection(this);
        }

        private void ApplyDrag()
        {
            var fwdSpeed = GetForwardSpeed();
            var drag = -fwdSpeed * Mathf.Abs(fwdSpeed) * this.drag;

            var force = drag * referenceWeight / Rigidbody.mass;
            
            Rigidbody.AddForce(DriverDirection * force);
        }

        private void ApplyCorrectiveForce()
        {
            // Calculate pose of front wheel assembly
            var th = PositionOnSpline;
            var hardTrackPosition = segment.SamplePoint(th);

            var ts = softAnchorPositionOnSpline;
            var softTrackPosition = segment.SamplePoint(ts);

            var center = (hardTrackPosition + softTrackPosition) / 2.0f;
            var normal = (hardTrackPosition - softTrackPosition).normalized;

            var alignmentDot = Vector3.Dot(normal, transform.forward);
            if (alignmentDot < 0.0f) normal = -normal;

            DriverDirection = normal;
            
            ApplyCorrectiveForce(center, normal);
            
            var orientation = CalculateOrientation(normal);
            ApplyCorrectiveTorque(orientation);
        }

        private void ApplyCorrectiveForce(Vector3 position, Vector3 direction)
        {
            // Calculate alignment delta as a force
            var force = (position - Rigidbody.position) * retentionSpring;

            // Calculate damping force
            var normalVelocity = Rigidbody.velocity;
            force -= normalVelocity * retentionDamping;

            var onTrack = PositionOnSpline >= 0.0f && PositionOnSpline <= 1.0f && softAnchorPositionOnSpline >= 0.0f && softAnchorPositionOnSpline <= 1.0f;
            
            // Apply Force
            if (onTrack) force -= direction * Vector3.Dot(direction, force);
            Rigidbody.AddForce(force, ForceMode.Acceleration);
            TangentialForce = transform.InverseTransformVector(force);
        }

        private Quaternion CalculateOrientation(Vector3 direction)
        {
            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            var lean = Mathf.Asin(Vector3.Dot(transform.right, direction)) * cornerLean * fwdSpeed;

            var rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(Vector3.forward * lean);
            return rotation;
        }

        private void ApplyCorrectiveTorque(Quaternion rotation)
        {
            var deltaRotation = CalculateDeltaRotation(rotation);

            var torque = (deltaRotation * retentionSpring - Rigidbody.angularVelocity * retentionDamping) * retentionTorqueConstant;
            Rigidbody.AddTorque(torque, ForceMode.Acceleration);
        }

        private Vector3 CalculateDeltaRotation(Quaternion rotation)
        {
            (rotation * Quaternion.Inverse(Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
            if (angle > 180.0f) angle -= 360.0f;
            axis.Normalize();
            if (!float.IsFinite(axis.x) || !float.IsFinite(axis.y) || !float.IsFinite(axis.z)) axis = Vector3.zero;

            var deltaRotation = axis * angle * Mathf.Deg2Rad;
            return deltaRotation;
        }

        public float GetForwardSpeed() => Vector3.Dot(DriverDirection, Rigidbody.velocity);
        
        private void OnValidate()
        {
            trainLength = Mathf.Max(0.0f, trainLength);
            Configure();
        }

        protected static float ToMps(float kmph) => kmph / 3.6f;
        protected static float ToKmpH(float mps) => mps * 3.6f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(HardAnchorPosition, SoftAnchorPosition);
        }
    }
}