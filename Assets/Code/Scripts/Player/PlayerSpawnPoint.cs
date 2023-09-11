using System;
using System.Runtime.CompilerServices;
using UnityEditor.Build;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerSpawnPoint : MonoBehaviour
    {
        public Bounds bounds;
        public Transform point;
        public bool defaultSpawn;

        private Animator animator;
        private static readonly int Occupied = Animator.StringToHash("occupied");

        // ReSharper disable once InconsistentNaming
        private static PlayerSpawnPoint _default;
        
        public Vector3 Position => (point ? point : transform).position;
        public Quaternion Orientation => (point ? point : transform).rotation;

        public static PlayerSpawnPoint Default
        {
            get
            {
                if (_default) return _default;
                
                var all = FindObjectsOfType<PlayerSpawnPoint>();
                foreach (var e in all)
                {
                    if (!e.defaultSpawn) continue;
                    
                    _default = e;
                    return e;
                }

                _default = all[0];
                _default.defaultSpawn = true;
                return _default;
            }
        }

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void FixedUpdate()
        {
            var occupied = false;
            var players = FindObjectsOfType<PlayerAvatar>();
            foreach (var p in players)
            {
                if (bounds.Contains(transform.InverseTransformPoint(p.transform.position)))
                {
                    occupied = true;
                    break;
                }
            }
            
            animator.SetBool(Occupied, occupied);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}