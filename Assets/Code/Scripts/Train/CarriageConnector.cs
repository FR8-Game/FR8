using System.Collections.Generic;
using System.Text.RegularExpressions;
using FR8.Interactions.Drivables;
using FR8.Utility;
using UnityEngine;

namespace FR8.Train
{
    public class CarriageConnector : MonoBehaviour, IDrivable
    {
        [SerializeField] private string key;
        [SerializeField] private CarriageConnectorSettings settings;
        [SerializeField] private Transform anchor;
        [SerializeField] private bool engaged = true;

        private new Rigidbody rigidbody;
        private CarriageConnector connection;
        
        public string Key => key;

        private static HashSet<CarriageConnector> all = new();

        private void Awake()
        {
            rigidbody = GetComponentInParent<Rigidbody>();
        }

        private void OnEnable()
        {
            all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        private void FixedUpdate()
        {
            if (!engaged)
            {
                connection = null;
                return;
            }
            
            if (connection)
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

                var direction = connection.transform.position - transform.position;
                var orientation = Quaternion.LookRotation(direction, rigidbody.transform.up);
                //transform.rotation = orientation;
                return;
            }

            var connectorsInRange = GetAllConnectorsInRange();
            foreach (var other in connectorsInRange)
            {
                Debug.DrawLine(anchor.position, other.anchor.position, Color.blue);
                var vector = other.anchor.position - anchor.position;
                var dist = vector.magnitude;
                var direction = vector / dist;
                
                if (dist < settings.connectionDistance)
                {
                    Connect(other);
                    return;
                }

                var force = direction * settings.forceScale * settings.forceFalloff.Evaluate(dist / settings.forceRange);
                rigidbody.AddForce(force);
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
            if (!anchor) anchor = Hierarchy.FindOrCreate(transform, new Regex(@".*(anchor|connect|point).*", RegexOptions.Compiled | RegexOptions.IgnoreCase), "Anchor");
        }

        private void OnDrawGizmosSelected()
        {
            if (!anchor || !settings) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(anchor.position, settings.forceRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(anchor.position, settings.connectionDistance);
        }
        
        public void OnValueChanged(float newValue)
        {
            engaged = newValue > 0.5f;
        }
    }
}
