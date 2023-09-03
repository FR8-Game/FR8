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

        public Vector3 Position => (point ? point : transform).position;
        public Quaternion Orientation => (point ? point : transform).rotation;
        public static PlayerSpawnPoint Default { get; private set; }

        private void Awake()
        {
            FindDefaultSpawnPoint();

            animator = GetComponentInChildren<Animator>();
        }

        private void OnValidate()
        {
            if (defaultSpawn) Default = this;
            FindDefaultSpawnPoint();
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

        private static void FindDefaultSpawnPoint()
        {
            var spawnPoints = FindObjectsOfType<PlayerSpawnPoint>();
            if (spawnPoints.Length == 0)
            {
                Default = null;
                return;
            }
            
            foreach (var e in spawnPoints)
            {
                if (Default != e) e.defaultSpawn = false;
                if (e.defaultSpawn && !Default) Default = e;
            }

            if (!Default)
            {
                Default = spawnPoints[0];
            }
        }
    }
}