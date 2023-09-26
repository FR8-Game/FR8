using UnityEngine;

namespace FR8Runtime.Train.Track
{
    public static class TrackUtility
    {
        public static (TrackSegment, float) FindClosestSegment(Vector3 position)
        {
            var best = (TrackSegment)null;
            var bestScore = float.MaxValue;
            var bestT = 0.0f;
            
            var segments = Object.FindObjectsOfType<TrackSegment>();
            foreach (var segment in segments)
            {
                var t = Mathf.Clamp01(segment.GetClosestPoint(position));
                
                var closestPoint = segment.SamplePoint(t);
                var score = (closestPoint - position).sqrMagnitude;

                if (score > bestScore) continue;

                bestScore = score;
                best = segment;
                bestT = t;
            }

            return (best, bestT);
        }
    }
}