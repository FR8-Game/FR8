using UnityEngine;

namespace FR8.Runtime.Train.Track
{
    public class TrackRunner
    {
        public TrackSegment CurrentTrack { get; private set; }
        public float T { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Direction { get; private set; }
        
        public TrackRunner(Vector3 startingPosition, TrackSegment startingTrack)
        {
            CurrentTrack = startingTrack;
            FixedUpdate(startingPosition);
        }

        public TrackRunner(Vector3 startingPosition)
        {
            FindNewTrack(startingPosition);
            FixedUpdate(startingPosition);
        }

        public void FindNewTrack(Vector3 position)
        {
            var bestScore = float.MaxValue;

            var segments = Object.FindObjectsOfType<TrackSegment>();
            foreach (var e in segments)
            {
                var t = e.GetClosestPoint(position, true);
                var closestPoint = e.SamplePoint(t);
                var distance = (closestPoint - position).magnitude;
                
                if (distance > bestScore) continue;

                bestScore = distance;
                CurrentTrack = e;
            }
        }
        
        public void FixedUpdate(Vector3 newPosition)
        {
            if (!CurrentTrack) FindNewTrack(newPosition);
            if (!CurrentTrack) return;

            var newTrack = CurrentTrack;
            if (CurrentTrack.IsOffStartOfTrack(newPosition)) newTrack = CurrentTrack.GetNextTrackStart();
            if (CurrentTrack.IsOffEndOfTrack(newPosition)) newTrack = CurrentTrack.GetNextTrackEnd();
            if (newTrack != null) CurrentTrack = newTrack;

            T = CurrentTrack.GetClosestPoint(newPosition, true);
            Position = CurrentTrack.SamplePoint(T);
            Direction = CurrentTrack.SampleVelocity(T).normalized;
        }
    }
}