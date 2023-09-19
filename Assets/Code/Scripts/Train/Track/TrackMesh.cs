using UnityEngine;

namespace FR8Runtime.Train.Track
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrackSegment))]
    public sealed partial class TrackMesh : MonoBehaviour
    {
        public partial void Clear();
        public partial void BakeMesh();
        
        #if !UNITY_EDITOR
        public partial void Clear() {}
        public partial void BakeMesh(){}
        #endif
    }
}