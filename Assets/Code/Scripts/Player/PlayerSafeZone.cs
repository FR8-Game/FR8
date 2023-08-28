using System.Collections.Generic;
using UnityEngine;

namespace FR8Runtime.Player
{
    public class PlayerSafeZone : MonoBehaviour, IVitalityBooster
    {
        [SerializeField] private Bounds bounds;

        public static readonly List<PlayerSafeZone> All = new();

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        public bool CanUse(PlayerAvatar avatar) => bounds.Contains(transform.InverseTransformPoint(avatar.Center));
        public void Bind(PlayerAvatar avatar) { }
        public void Unbind(PlayerAvatar avatar) { }
    }
}
