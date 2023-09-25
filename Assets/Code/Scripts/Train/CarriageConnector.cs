using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FR8Runtime.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8Runtime.Train
{
    public class CarriageConnector : MonoBehaviour, IInteractable
    {
        [SerializeField] private CarriageConnectorSettings settings;
        [SerializeField] private bool engaged = true;
        [SerializeField] private Transform anchor;
        [SerializeField] private Transform magnetFX;
        [SerializeField] private float fxTransitionSpeed;
        [SerializeField] private Vector3 offset;

        private Vector3 anchorOrigin;
        private float anchorConnectionDistance;
        private new Rigidbody rigidbody;

        private float magnetFXPercent;
        private Vector3 magnetFXScale;

        private static HashSet<CarriageConnector> all = new();

        public bool CanInteract => true;
        public string DisplayName => "Carriage Connector";
        public string DisplayValue => engaged ? "Engaged" : "Disengaged";
        public bool OverrideInteractDistance { get; }
        public float InteractDistance { get; }
        public CarriageConnector Connection { get; private set; }

        public Vector3 AnchorPosition => transform.TransformPoint(anchorOrigin);
        public TrainCarriage Carriage { get; set; }

        public void Nudge(int direction)
        {
            engaged = direction > 0;
        }

        public void BeginInteract(GameObject interactingObject)
        {
            engaged = !engaged;
        }

        public void ContinueInteract(GameObject interactingObject) { }

        protected void Awake()
        {
            Carriage = GetComponentInParent<TrainCarriage>();

            if (magnetFX)
            {
                magnetFXScale = magnetFX.transform.localScale;
            }

            anchorOrigin = transform.InverseTransformPoint(anchor.position);
            anchorConnectionDistance = anchorOrigin.magnitude;
        }

        private void OnEnable()
        {
            all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        private void Start()
        {
            rigidbody = Carriage.Body;
        }

        protected void FixedUpdate()
        {
            UpdateFX(engaged);

            if (!engaged)
            {
                Connection = null;
                return;
            }

            if (Connection)
            {
                if (!Connection.engaged)
                {
                    Connection = null;
                    return;
                }
                
                ApplyForce();
                PositionAnchor(Connection.transform, 1.0f);

                return;
            }

            var connectorsInRange = GetAllConnectorsInRange();
            foreach (var other in connectorsInRange)
            {
                var vector = other.transform.position - transform.position;
                var dist = vector.magnitude - (anchorConnectionDistance + other.anchorConnectionDistance);
                var direction = vector / dist;

                if (!other.engaged) continue;
                if (TryConnect(dist, other)) break;

                ApplyMagneticForce(direction, dist);
                PositionAnchor(other.transform, 1.0f - (dist / settings.forceRange));
                
                break;
            }
        }

        private void PositionAnchor(Transform target, float t)
        {
            var vector = target.transform.position - transform.position;
            vector = vector.normalized * Mathf.Min(vector.magnitude, anchorConnectionDistance);
            
            anchor.position = Vector3.Lerp(transform.TransformPoint(anchorOrigin), transform.position + vector, t);
            anchor.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vector, rigidbody.transform.up), t)* Quaternion.Euler(offset);
        }

        private bool TryConnect(float dist, CarriageConnector other)
        {
            if (dist < settings.connectionDistance)
            {
                Connect(other);
                return true;
            }

            return false;
        }

        private void ApplyMagneticForce(Vector3 direction, float dist)
        {
            var force = direction * settings.forceScale * settings.forceFalloff.Evaluate(dist / settings.forceRange);
            rigidbody.AddForce(force);
        }

        private void ApplyForce()
        {
            var displacement = Connection.transform.position - transform.position;
            displacement = displacement.normalized * (displacement.magnitude - anchorConnectionDistance - Connection.anchorConnectionDistance);

            var mass = rigidbody.mass;
            var otherMass = Connection.rigidbody.mass;
            var totalMass = mass + otherMass;

            var force = displacement / (Time.deltaTime * Time.deltaTime);
            force -= rigidbody.velocity / Time.deltaTime;
            force *= mass * (mass / totalMass);

            force = transform.forward * Vector3.Dot(transform.forward, force);

            rigidbody.AddForce(force / 2.0f);
            Connection.rigidbody.AddForce(-force / 2.0f);
        }

        private void UpdateFX(bool engaged)
        {
            if (magnetFX)
            {
                magnetFXPercent += ((engaged ? 1.0f : 0.0f) - magnetFXPercent) * fxTransitionSpeed * Time.deltaTime;
                magnetFX.transform.localScale = magnetFXScale * magnetFXPercent;
            }
        }

        private List<CarriageConnector> GetAllConnectorsInRange()
        {
            var list = new List<CarriageConnector>();
            foreach (var e in all)
            {
                if (e == this) continue;

                if ((e.AnchorPosition - AnchorPosition).sqrMagnitude < settings.forceRange * settings.forceRange)
                {
                    list.Add(e);
                }
            }

            return list;
        }

        public void Connect(CarriageConnector other)
        {
            if (other == this) return;

            Debug.Log($"{Carriage.name} Connected to {other.Carriage.name}");
            other.Connection = this;
            Connection = other;
        }

        public void Disconnect()
        {
            Debug.Log($"{Carriage.name} Disconnected from {Connection.Carriage.name}");
            Connection.Connection = null;
            Connection = null;
        }

        private void OnValidate()
        {
            if (!anchor) anchor = CodeUtility.HierarchyUtility.FindOrCreate(transform, new Regex(@".*(anchor|connect|point).*", RegexOptions.Compiled | RegexOptions.IgnoreCase), "Anchor");
        }

        private void OnDrawGizmosSelected()
        {
            if (!anchor || !settings) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(AnchorPosition, settings.forceRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AnchorPosition, settings.connectionDistance);
        }
    }
}