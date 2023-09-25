using System.Collections.Generic;
using System.Text;
using FR8Runtime.Interactions.Drivers;
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
        [SerializeField][Range(0.0f, 1.0f)] private float handbrakeDefault = 1.0f;
        [SerializeField] private float handbrakeConstant;
        [SerializeField] private AnimationCurve handbrakeEfficiencyCurve = new
        (
            new Keyframe(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
            new Keyframe(0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
            new Keyframe(4.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)
        );

        [Space]
        [SerializeField] protected float retentionSpring = 2500.0f;

        [SerializeField] protected float retentionDamping = 50.0f;
        [SerializeField] protected float retentionTorqueConstant = 0.2f;

        [SerializeField] protected float trainLength = 20.0f;

        protected TrackSegment segment;
        private List<CarriageConnector> carriageConnectors;

        private float softAnchorPositionOnSpline;

        public TrackSegment Segment
        {
            get => segment;
            set => segment = value;
        }

        public DriverNetwork DriverNetwork { get; private set; }
        public string Name => name;
        public Rigidbody Body { get; private set; }
        public Vector3 DriverDirection { get; private set; }
        public float ReferenceWeight => referenceWeight;
        public Vector3 TangentialForce { get; private set; }
        public float PositionOnSpline { get; private set; }
        public float LastPositionOnSpline { get; private set; }
        public List<TrainCarriage> ConnectedCarriages { get; } = new();

        public bool Stationary { get; private set; }

        public Vector3 HardAnchorPosition => (Body ? Body.position : transform.position) + transform.forward * trainLength * 0.5f;
        public Vector3 SoftAnchorPosition => (Body ? Body.position : transform.position) - transform.forward * trainLength * 0.5f;

        public static readonly List<TrainCarriage> All = new();

        public const string HandbrakeKey = "handbrake";
        
        private void Awake()
        {
            Configure();

            carriageConnectors = new List<CarriageConnector>(GetComponentsInChildren<CarriageConnector>());
            DriverNetwork = GetComponent<DriverNetwork>();
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        protected virtual void Start()
        {
            FindClosestSegment();
            DriverNetwork.SetValue(HandbrakeKey, handbrakeDefault);
        }

        public void FindClosestSegment()
        {
            float t;
            (segment, t) = TrackUtility.FindClosestSegment(transform.position);

            transform.position = segment.SamplePoint(t);
            transform.rotation = Quaternion.LookRotation(segment.SampleTangent(t), Vector3.up);
        }

        protected virtual void Configure()
        {
            Body = GetComponent<Rigidbody>();
            if (!Application.isPlaying)
            {
                referenceWeight = Body.mass;
            }
        }

        protected virtual void FixedUpdate()
        {
            FindConnectedCarriages(ConnectedCarriages);
            
            LastPositionOnSpline = PositionOnSpline;
            PositionOnSpline = segment.GetClosestPoint(HardAnchorPosition);
            softAnchorPositionOnSpline = segment.GetClosestPoint(SoftAnchorPosition);

            ApplyDrag();
            ApplyHandbrake();
            ApplyCorrectiveForce();

            Stationary = IsStationary();

            if (segment) segment.UpdateConnection(this);

        }

        private bool IsStationary()
        {
            var isHandbrakeOn = false;
            foreach (var e in ConnectedCarriages)
            {
                if (e is Locomotive) return false;
            }
                
            foreach (var e in ConnectedCarriages)
            {
                if (e.DriverNetwork.GetValue(HandbrakeKey) < 0.95f) continue;
                isHandbrakeOn = true;
                break;
            }

            if (!isHandbrakeOn) return false;

            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            if (fwdSpeed > 0.1f) return false;
            
            return true;
        }

        private void ApplyHandbrake()
        {
            var speed = GetForwardSpeed();
            var force = DriverDirection * -speed * Mathf.Clamp01(GetHandbrakeConstant() * DriverNetwork.GetValue(HandbrakeKey)) * handbrakeConstant;

            Body.AddForce(force * referenceWeight);
        }

        public float GetHandbrakeConstant()
        {
            var speed = Mathf.Abs(GetForwardSpeed());
            return handbrakeEfficiencyCurve.Evaluate(speed);
        }

        private void FindConnectedCarriages(List<TrainCarriage> list)
        {
            list.Clear();
            list.Add(this);

            var openList = new Queue<CarriageConnector>(carriageConnectors);
            while (openList.Count > 0)
            {
                var h = openList.Dequeue();

                if (!h.Connection) continue;

                var other = h.Connection.Carriage;
                if (list.Contains(other)) continue;

                list.Add(other);
                foreach (var c in other.carriageConnectors)
                {
                    if (c == h) continue;
                    if (c == h.Connection) continue;
                    openList.Enqueue(c);
                }
            }
        }

        private void ApplyDrag()
        {
            var fwdSpeed = GetForwardSpeed();
            var drag = -fwdSpeed * Mathf.Abs(fwdSpeed) * this.drag;

            var force = drag * referenceWeight / Body.mass;

            Body.AddForce(DriverDirection * force);
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
            var force = (position - Body.position) * retentionSpring;

            // Calculate damping force
            var normalVelocity = Body.velocity;
            force -= normalVelocity * retentionDamping;

            // Apply Force
            force -= direction * Vector3.Dot(direction, force);
            Body.AddForce(force, ForceMode.Acceleration);
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

            var torque = (deltaRotation * retentionSpring - Body.angularVelocity * retentionDamping) * retentionTorqueConstant;
            Body.AddTorque(torque, ForceMode.Acceleration);
        }

        private Vector3 CalculateDeltaRotation(Quaternion rotation)
        {
            (rotation * Quaternion.Inverse(Body.rotation)).ToAngleAxis(out var angle, out var axis);
            if (angle > 180.0f) angle -= 360.0f;
            axis.Normalize();
            if (!float.IsFinite(axis.x) || !float.IsFinite(axis.y) || !float.IsFinite(axis.z)) axis = Vector3.zero;

            var deltaRotation = axis * angle * Mathf.Deg2Rad;
            return deltaRotation;
        }

        public float GetForwardSpeed() => Vector3.Dot(DriverDirection, Body.velocity);

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

        public string GetDebugInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"[{GetType().Name}]{name}");
            
            if (Application.isPlaying) AppendDebugInfoPlaymode(sb);
            else AppendDebugInfo(sb);
            
            sb.Append(new string('-', 10));
            
            return sb.ToString();
        }

        protected virtual void AppendDebugInfo(StringBuilder sb)
        {
            
        }

        protected virtual void AppendDebugInfoPlaymode(StringBuilder sb)
        {
            sb.AppendLine($"Handbrake Efficiency: {GetHandbrakeConstant() * 100.0f,3:N0}%");
        }
    }
}