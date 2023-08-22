using System.Collections.Generic;
using UnityEngine;

namespace FR8.Rendering
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public class MapMarker : MonoBehaviour
    {
        [SerializeField] private Color markerColor;
        
        private MeshFilter filter;
        
        public static List<MapMarker> All { get; } = new();
        public Mesh Mesh => filter.sharedMesh;
        public Color MarkerColor
        {
            get => markerColor;
            set => markerColor = value;
        }

        private void OnEnable()
        {
            filter = GetComponent<MeshFilter>();
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void OnDrawGizmosSelected()
        {
            var filter = GetComponent<MeshFilter>();
            if (!filter) return;

            var mesh = filter.sharedMesh;
            if (!mesh) return;

            Gizmos.color = markerColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireMesh(mesh);
        }
    }
}