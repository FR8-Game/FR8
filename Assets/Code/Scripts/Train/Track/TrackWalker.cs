using System;
using UnityEngine;

namespace FR8.Train.Track
{
    [System.Serializable]
    public class TrackWalker
    {
        [SerializeField] private TrackSegment currentSegment;
        [SerializeField] private float t;

        public Func<TrackSegment, Vector3, float> sampleMethod = (segment, point) => segment.GetClosestPoint(point);

        public TrackSegment CurrentSegment
        {
            get => currentSegment;
            set => currentSegment = value;
        }
        public Vector3 Position { get; private set; }
        public Vector3 Tangent { get; private set; }
        
        public void Walk(Vector3 newPosition)
        {
            t = sampleMethod(currentSegment, newPosition);
            Position = currentSegment.SamplePoint(t);
            Tangent = currentSegment.SampleTangent(t).normalized;
        }

        public static implicit operator bool(TrackWalker walker)
        {
            if (walker == null) return false;
            if (!walker.currentSegment) return false;

            return true;
        }
    }
}