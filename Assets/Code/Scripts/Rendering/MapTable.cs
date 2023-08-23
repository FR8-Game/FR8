using System.Collections.Generic;
using UnityEngine;

namespace FR8Runtime.Rendering
{
    [ExecuteAlways]
    public class MapTable : MonoBehaviour
    {
        [SerializeField] private MeshFilter mapGraphicsRenderer;
        [SerializeField] private Camera mapCamera;

        public bool Draw => mapGraphicsRenderer && mapCamera;
        public Transform MapTransform => mapGraphicsRenderer ? mapGraphicsRenderer.transform : null;
        public Bounds MapBounds => mapGraphicsRenderer ? mapGraphicsRenderer.sharedMesh.bounds : default;
        public Camera MapCamera => mapCamera;

        public static List<MapTable> All { get; } = new();

        private void OnEnable()
        {
            All.Add(this);
            if (!mapCamera) mapCamera = GetComponentInChildren<Camera>();
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void OnDrawGizmos()
        {
            if (!mapGraphicsRenderer) return;
            
            Gizmos.color = Color.cyan;
            Gizmos.matrix = MapTransform.localToWorldMatrix;
            Gizmos.DrawWireCube(MapBounds.center, MapBounds.size);
        }
    }
}