using System.Collections.Generic;
using FR8Runtime.Rendering;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerTetherPoint : MonoBehaviour, IVitalityBooster
    {
        [SerializeField] private float range = 20.0f;
        [SerializeField] private float angleLimit = 90.0f;

        private RopeRenderer rope;
        private PlayerAvatar binding;

        public static readonly List<PlayerTetherPoint> All = new();

        private void Awake()
        {
            rope = GetComponentInChildren<RopeRenderer>();
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void Update()
        {
            if (rope)
            {
                if (binding)
                {
                    rope.enabled = true;
                    rope.endTarget.position = binding.Center;

                    var orientation = Quaternion.LookRotation(binding.Center - transform.position, Vector3.up);
                    rope.startTarget.rotation = orientation;
                    rope.endTarget.rotation = orientation;
                }
                else
                {
                    rope.enabled = false;
                }
            }
        }

        public bool CanUse(PlayerAvatar avatar)
        {
            if ((avatar.Center - transform.position).sqrMagnitude > range * range) return false;
            if (Mathf.Acos(Vector3.Dot(avatar.Center - transform.position, transform.forward)) * Mathf.Rad2Deg > angleLimit) return false;

            return true;
        }

        public void Bind(PlayerAvatar avatar)
        {
            binding = avatar;
        }

        public void Unbind(PlayerAvatar avatar)
        {
            binding = null;
        }

        private void OnValidate()
        {
            range = Mathf.Max(range, 0.0f);
            angleLimit = Mathf.Clamp(angleLimit, 0.0f, 180.0f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;

            drawSlice(0.0f);
            drawSlice(90.0f);

            var res = 100;
            var tau = Mathf.PI * 2.0f;

            for (var i = 0; i < res; i++)
            {
                var a0 = (float)i / res * tau;
                var a1 = (i + 1.0f) / res * tau;

                var s = Mathf.Sin(angleLimit * Mathf.Deg2Rad);
                var c = Mathf.Cos(angleLimit * Mathf.Deg2Rad);

                Gizmos.DrawLine(
                    new Vector3(Mathf.Cos(a0) * s, Mathf.Sin(a0) * s, c) * range,
                    new Vector3(Mathf.Cos(a1) * s, Mathf.Sin(a1) * s, c) * range
                );
            }

            void drawSlice(float roll)
            {
                Gizmos.DrawLine(Vector3.zero, getEnd(roll, -angleLimit));
                Gizmos.DrawLine(Vector3.zero, getEnd(roll, angleLimit));

                var res = 100;
                for (var i = 0; i < res; i++)
                {
                    var a0 = ((float)i / res * 2.0f - 1.0f) * angleLimit;
                    var a1 = ((i + 1.0f) / res * 2.0f - 1.0f) * angleLimit;

                    Gizmos.DrawLine(
                        getEnd(roll, a0),
                        getEnd(roll, a1));
                }
            }

            Vector3 getEnd(float roll, float angle)
            {
                return Quaternion.Euler(0.0f, 0.0f, roll) * Quaternion.Euler(angle, 0.0f, 0.0f) * Vector3.forward * range;
            }
        }
    }
}