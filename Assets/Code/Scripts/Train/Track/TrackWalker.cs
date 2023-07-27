using UnityEngine;

namespace FR8.Train.Track
{
    [System.Serializable]
    public class TrackWalker
    {
        [SerializeField] private TrackSegment currentSegment;

        public TrackSegment CurrentSegment
        {
            get => currentSegment;
            set => currentSegment = value;
        }
        public Vector3 Position { get; private set; }
        public Vector3 Tangent { get; private set; }
        
        public void Walk(Vector3 newPosition)
        {
            var closestPoint = currentSegment.GetClosestPoint(newPosition);
            Position = currentSegment.SamplePoint(closestPoint);
            Tangent = currentSegment.SampleTangent(closestPoint).normalized;
        }

        public static implicit operator bool(TrackWalker walker)
        {
            if (walker == null) return false;
            if (!walker.currentSegment) return false;

            return true;
        }
    }
}