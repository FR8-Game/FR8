using System.Collections.Generic;
using System.Text.RegularExpressions;
using FR8.Interactions.Drivers;
using UnityEngine;

namespace FR8.Train
{
    public class CarriageConnector : Driver
    {
        [SerializeField] private CarriageConnectorSettings settings;
        [SerializeField] private Transform anchor;
        [SerializeField] private Transform magnetFX;
        [SerializeField] private float fxTransitionSpeed;

        private new Rigidbody rigidbody;
        private CarriageConnector connection;

        private float magnetFXPercent;
        private Vector3 magnetFXScale;

        private static HashSet<CarriageConnector> all = new();

        public override string DisplayValue => Value > 0.5f ? "Engaged" : "Disengaged";

        protected override void Awake()
        {
            base.Awake();
            rigidbody = GetComponentInParent<Rigidbody>();

            if (magnetFX)
            {
                magnetFXScale = magnetFX.transform.localScale;
            }
        }

        private void OnEnable()
        {
            all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            var engaged = Value > 0.5f;

            UpdateFX(engaged);

            if (!engaged) return;

            if (connection)
            {
                if (connection.Value < 0.5f)
                {
                    SetValue(0.0f);
                    connection = null;
                    return;
                }
                
                ApplyForce();
                UpdateOrientation();
                
                return;
            }

            var connectorsInRange = GetAllConnectorsInRange();
            foreach (var other in connectorsInRange)
            {
                var vector = other.anchor.position - anchor.position;
                var dist = vector.magnitude;
                var direction = vector / dist;

                if (other.Value < 0.5f) continue;
                if (TryConnect(dist, other)) break;

                ApplyMagneticForce(direction, dist);
            }
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

        private void UpdateOrientation()
        {
            var direction = connection.transform.position - transform.position;
            var orientation = Quaternion.LookRotation(direction, rigidbody.transform.up);
            //transform.rotation = orientation;
        }

        private void ApplyForce()
        {
            var displacement = connection.anchor.position - anchor.position;

            var mass = rigidbody.mass;
            var otherMass = connection.rigidbody.mass;
            var totalMass = mass + otherMass;

            var force = displacement * settings.connectionForce;
            force -= rigidbody.velocity * settings.connectionDamping;
            force *= mass * (mass / totalMass);

            force = transform.forward * Vector3.Dot(transform.forward, force);

            rigidbody.AddForce(force);
            connection.rigidbody.AddForce(-force);
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

                if ((e.anchor.position - anchor.position).sqrMagnitude < settings.forceRange * settings.forceRange)
                {
                    list.Add(e);
                }
            }

            return list;
        }

        public void Connect(CarriageConnector other)
        {
            if (other == this) return;

            other.connection = this;
            connection = other;
        }

        public void Disconnect()
        {
            connection.connection = null;
            connection = null;
        }

        private void OnValidate()
        {
            if (!anchor) anchor = Utility.Hierarchy.FindOrCreate(transform, new Regex(@".*(anchor|connect|point).*", RegexOptions.Compiled | RegexOptions.IgnoreCase), "Anchor");
        }

        private void OnDrawGizmosSelected()
        {
            if (!anchor || !settings) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(anchor.position, settings.forceRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(anchor.position, settings.connectionDistance);
        }

        protected override void SetValue(float newValue) => base.SetValue(newValue > 0.5f ? 1.0f : 0.0f);

        public override void Nudge(int direction)
        {
            SetValue(direction == 1 ? 1.0f : 0.0f);
        }

        public override void BeginInteract(GameObject interactingObject)
        {
            SetValue(1.0f - Value);
        }

        public override void ContinueInteract(GameObject interactingObject) { }
    }
}