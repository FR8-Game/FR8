using System.Collections.Generic;
using UnityEngine;

namespace FR8
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrackPoint : MonoBehaviour
    {
        [SerializeField] private List<TrackPoint> connections = new();

        public List<TrackPoint> Connections => connections;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var c in connections)
            {
                Gizmos.DrawLine(transform.position, c.transform.position);
            }
        }
    }
}
