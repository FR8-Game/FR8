using UnityEngine;

namespace FR8.Track
{
    [System.Serializable]
    public class TrackWalker
    {
        [SerializeField] private TrackSegment currentSegment;

        public Vector3 Position { get; private set; }
        public Vector3 Tangent { get; private set; }
        
        public void Walk(Vector3 newPosition)
        {
            var closestPoint = currentSegment.GetClosestPoint(newPosition);
            Position = currentSegment.SamplePoint(closestPoint);
            Tangent = currentSegment.SampleTangent(closestPoint);
        }
    }
}