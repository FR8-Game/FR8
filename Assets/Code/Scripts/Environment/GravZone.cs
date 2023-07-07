using System;
using System.Collections.Generic;
using UnityEngine;

namespace FR8.Environment
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class GravZone : MonoBehaviour
    {
        [SerializeField] private Bounds bounds;
        [SerializeField] private Vector3 gravity;
        [SerializeField] private Mode mode;

        private readonly Dictionary<Rigidbody, Vector3> affectedBodies = new();
        private static readonly HashSet<GravZone> all = new();

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
            affectedBodies.Clear();

            var broadPhase = Physics.OverlapBox(transform.position + transform.rotation * bounds.center, bounds.extents, transform.rotation);
            foreach (var e in broadPhase)
            {
                if (!e.attachedRigidbody) continue;
                if (!e.attachedRigidbody.useGravity) continue;
                if (!bounds.Contains(e.attachedRigidbody.position)) continue;
                if (affectedBodies.ContainsKey(e.attachedRigidbody)) continue;

                affectedBodies.Add(e.attachedRigidbody, GetForce(e.attachedRigidbody));
            }

            foreach (var pair in affectedBodies)
            {
                pair.Key.AddForce(pair.Value, ForceMode.Acceleration);
            }
        }

        public enum Mode
        {
            Directional,
            Spherical,
        }

        private Vector3 GetForce(Rigidbody body)
        {
            switch (mode)
            {
                case Mode.Directional:
                    return transform.rotation * gravity;
                case Mode.Spherical:
                    var vector = transform.position - body.position;
                    return vector.normalized / vector.sqrMagnitude * gravity.y;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsGravityAffected(Rigidbody body, out Vector3 gravity)
        {
            gravity = Vector3.zero;
            var res = false;

            foreach (var zone in all)
            {
                if (!zone.affectedBodies.ContainsKey(body)) continue;
                gravity += zone.affectedBodies[body];
                res = true;
            }

            return res;
        }

        private void OnDrawGizmos()
        {
            DrawGizmos(0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmos(1.0f);

            if (!Application.isPlaying) return;
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.identity;

            foreach (var body in affectedBodies)
            {
                Gizmos.DrawLine(transform.position, body.Key.position);
                Gizmos.DrawRay(body.Key.position, body.Value);
            }
        }

        private void DrawGizmos(float alpha)
        {
            void DrawArrow(Vector3 start, Vector3 end)
            {
                var back = (start - end).normalized;
                var right1 = Vector3.Cross(back, Vector3.right); 
                var right2 = Vector3.Cross(back, Vector3.forward);
                var right = right1.sqrMagnitude > right2.sqrMagnitude ? right1 : right2;
                var headSize = (start - end).magnitude * 0.2f;

                var p1 = end + back * headSize;
                var p2 = end + (back + right * 0.5f) * headSize;
                var p3 = end + (back - right * 0.5f) * headSize;

                Gizmos.DrawLine(start, p1);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, end);
                Gizmos.DrawLine(end, p2);
            }

            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, alpha);
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireCube(bounds.center, bounds.size);
            var c = Gizmos.color;
            Gizmos.color = new Color(c.r, c.g, c.b, c.a * 0.1f);
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = c;

            var grav = gravity * 0.25f;
            
            switch (mode)
            {
                case Mode.Directional:
                    DrawArrow(-grav / 2.0f, grav / 2.0f);
                    break;
                case Mode.Spherical:
                    DrawArrow(Vector3.up * grav.y, Vector3.zero);
                    DrawArrow(Vector3.down * grav.y, Vector3.zero);
                    DrawArrow(Vector3.left * grav.y, Vector3.zero);
                    DrawArrow(Vector3.right * grav.y, Vector3.zero);
                    DrawArrow(Vector3.back * grav.y, Vector3.zero);
                    DrawArrow(Vector3.forward * grav.y, Vector3.zero);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}