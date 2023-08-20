using System;
using UnityEngine;

namespace FR8.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public class DisableFrustumCulling : MonoBehaviour
    {
        private void OnEnable()
        {
            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 999999.0f);
        }
    }
}