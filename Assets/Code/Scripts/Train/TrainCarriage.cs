using System.Collections.Generic;
using System.Text;
using FMOD.Studio;
using FR8.Runtime.Interactions.Drivers;
using FR8.Runtime.References;
using FR8.Runtime.Train.Track;
using Unity.Collections;
using UnityEngine;

namespace FR8.Runtime.Train
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class TrainCarriage : MonoBehaviour, INameplateProvider
    {
        public const int TrainLayer = 10;

        public TrainCarriageSettings carriageSettings;
        public float referenceWeight = 100000.0f;
        [Range(0.0f, 1.0f)] public float handbrakeDefault = 1.0f;
        public float trainLength = 10.0f;

        [Space]
        [ReadOnly][SerializeField] private float evaluatedBrakeLoad;
        
        protected TrackSegment segment;

        private Vector2 softAnchorPosition;
        private EventInstance brakeSound;
        private float handbrakeLoad;

        public TrackSegment Segment
        {
            get => segment;
            set
            {
                if (segment == value) return;
                Debug.Log($"{this} Jumped from {segment} to {value}");
                segment = value;
            }
        }

        public DriverNetwork DriverNetwork { get; private set; }
        public string Name => name;
        public Rigidbody Body { get; private set; }
        public Vector3 DriverDirection { get; private set; }
        public Vector3 TangentialForce { get; private set; }

        public float PositionOnSpline { get; private set; }
        public float LastPositionOnSpline { get; private set; }
        public List<TrainCarriage> ConnectedCarriages { get; } = new();
        public List<CarriageConnector> CarriageConnectors { get; private set; }

        public bool Stationary { get; private set; }

        //public Vector3 HardAnchorPosition => (Body ? Body.position : transform.position) + transform.forward * trainLength * 0.5f;
        //public Vector3 SoftAnchorPosition => (Body ? Body.position : transform.position) - transform.forward * trainLength * 0.5f;

        public static readonly List<TrainCarriage> All = new();

        public const string HandbrakeKey = "handbrake";
        
        private void Awake()
        {
            Configure();

            brakeSound = SoundReference.TrainBrake.InstanceAndStart();

            CarriageConnectors = new List<CarriageConnector>(GetComponentsInChildren<CarriageConnector>());
            DriverNetwork = GetComponent<DriverNetwork>();

            gameObject.layer = TrainLayer;
            foreach (var e in GetComponentsInChildren<Transform>())
            {
                e.gameObject.layer = TrainLayer;
            }
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
            (Segment, t) = TrackUtility.FindClosestSegment(transform.position);

            transform.position = Segment.SamplePoint(t);
            transform.rotation = Quaternion.LookRotation(Segment.SampleTangent(t), Vector3.up);
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
            PositionOnSpline = Segment.GetClosestPoint(Body.position, true);

            ApplyDrag();
            ApplyHandbrake();
            ApplyCorrectiveForce();

            Stationary = IsStationary();

            if (Segment.IsOffStartOfTrack(Body.position))
            {
                offEnd(Segment.GetNextTrackStart());
            }
            else if (Segment.IsOffEndOfTrack(Body.position))
            {
                offEnd(Segment.GetNextTrackEnd());
            }
            
            evaluatedBrakeLoad = GetBrakeLoad();
            brakeSound.setParameterByName("BrakeLoad", evaluatedBrakeLoad);

            void offEnd(TrackSegment next)
            {
                if (next)
                {
                    Segment = next;
                    return;
                }

                var closest = Segment.SamplePoint(Segment.GetClosestPoint(Body.position, true));
                var displacement = closest - Body.position;
                var normal = displacement.normalized;
                Body.position = closest;
                Body.velocity += normal * Mathf.Max(0.0f, -Vector3.Dot(normal, Body.velocity));
            }
        }

        public bool IsStationary(List<Locomotive> connectedLocomotives = null)
        {
            var isHandbrakeOn = false;
            foreach (var e in ConnectedCarriages)
            {
                if (e is not Locomotive locomotive) continue;
                connectedLocomotives?.Add(locomotive);
            }

            if (connectedLocomotives != null && connectedLocomotives.Count > 0) return false;
                
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
            var force = DriverDirection * -speed * Mathf.Clamp01(GetHandbrakeConstant() * DriverNetwork.GetValue(HandbrakeKey)) * carriageSettings.handbrakeConstant;

            handbrakeLoad = Mathf.Abs(Vector3.Dot(DriverDirection, force));
            
            Body.AddForce(force * referenceWeight);
        }
        
        protected virtual float GetBrakeLoad() => handbrakeLoad;

        public float GetHandbrakeConstant()
        {
            var speed = Mathf.Abs(GetForwardSpeed());
            return carriageSettings.handbrakeEfficiencyCurve.Evaluate(speed);
        }

        private void FindConnectedCarriages(List<TrainCarriage> list)
        {
            list.Clear();
            list.Add(this);

            var openList = new Queue<CarriageConnector>(CarriageConnectors);
            while (openList.Count > 0)
            {
                var h = openList.Dequeue();

                if (!h.Connection) continue;

                var other = h.Connection.Carriage;
                if (list.Contains(other)) continue;

                list.Add(other);
                foreach (var c in other.CarriageConnectors)
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
            var drag = -fwdSpeed * Mathf.Abs(fwdSpeed) * carriageSettings.drag;

            var force = drag * referenceWeight / Body.mass;

            Body.AddForce(DriverDirection * force);
        }

        private void ApplyCorrectiveForce()
        {
            // Calculate pose of front wheel assembly
            
            var center = Segment.SamplePoint(PositionOnSpline);
            var normal = Segment.SampleTangent(PositionOnSpline);

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
            direction = direction.normalized;
            var force = (position - Body.position) / Time.deltaTime;

            // Calculate damping force
            var normalVelocity = Body.velocity;
            force -= normalVelocity;

            // Apply Force
            force -= direction * Vector3.Dot(direction, force);
            Body.velocity += force;
            TangentialForce = transform.InverseTransformVector(force);
        }

        private Quaternion CalculateOrientation(Vector3 direction)
        {
            var fwdSpeed = Mathf.Abs(GetForwardSpeed());
            var lean = Mathf.Asin(Vector3.Dot(transform.right, direction)) * carriageSettings.cornerLean * fwdSpeed;

            var rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(Vector3.forward * lean);
            return rotation;
        }

        private void ApplyCorrectiveTorque(Quaternion rotation)
        {
            var delta = rotation * Quaternion.Inverse(Body.rotation);
            delta.ToAngleAxis(out var angle, out var axis);
            if (!(float.IsFinite(axis.x) && float.IsFinite(axis.y) && float.IsFinite(axis.z)))
            {
                axis = Vector3.up;
                angle = 0.0f;
            }
            if (angle > 180.0f) angle -= 360.0f;
            if (angle < -180.0f) angle += 360.0f;
            Body.angularVelocity = axis * angle * Mathf.Deg2Rad / Time.deltaTime;
        }

        public float GetForwardSpeed() => Vector3.Dot(DriverDirection, Body.velocity);

        private void OnValidate()
        {
            trainLength = Mathf.Max(0.0f, trainLength);
            Configure();
        }

        protected static float ToMps(float kmph) => kmph / 3.6f;
        protected static float ToKmpH(float mps) => mps * 3.6f;

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