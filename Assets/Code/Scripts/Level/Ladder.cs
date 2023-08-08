using System;
using System.Security.Cryptography;
using FR8.Player;
using UnityEngine;

namespace FR8.Level
{
    public class Ladder : MonoBehaviour
    {
        [SerializeField] private float height;
        [SerializeField] private float heightOffset;
        [SerializeField] private float normalOffset;

        public Rigidbody ParentRigidbody { get; private set; }
        public float Height => height;
        public float HeightOffset => heightOffset;
        public Vector3 Velocity => ParentRigidbody ? ParentRigidbody.velocity : Vector3.zero;

        private void Awake()
        {
            ParentRigidbody = GetComponent<Rigidbody>();
        }

        public float FromWorldPos(Vector3 position)
        {
            var diff = position - transform.position;
            return Mathf.Clamp(diff.y - heightOffset, 0.0f, height);
        }

        public Vector3 ToWorldPos(float ladderPos)
        {
            ladderPos = Mathf.Clamp(ladderPos, 0.0f, height);
            return transform.position + transform.forward * normalOffset + Vector3.up * (ladderPos + heightOffset);
        }

        private void OnDrawGizmos() => DrawGizmos(false);
        private void OnDrawGizmosSelected() => DrawGizmos(true);

        private void DrawGizmos(bool selected)
        {
            var col = new Color(1.0f, 1.0f, 0.0f, selected ? 1.0f : 0.2f);
            Gizmos.color = col;
            var right = transform.right * 0.25f;

            Gizmos.DrawLine(ToWorldPos(0.0f) + right, ToWorldPos(height) + right);
            Gizmos.DrawLine(ToWorldPos(0.0f) - right, ToWorldPos(height) - right);
            for (var h = 0.5f; h < height - 0.5f; h += 0.25f)
            {
                Gizmos.DrawLine(ToWorldPos(h) + right, ToWorldPos(h) - right);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.transform.GetComponentInParent<PlayerGroundedAvatar>();
            if (!player) return;
            if (player.Ladder) return;

            player.SetLadder(this);
        }
    }
}