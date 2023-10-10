using UnityEngine;

namespace FR8Runtime.Train.Track
{
    public class TrackRunner
    {
        public TrackBake CurrentTrack { get; private set; }
        public float T { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Direction { get; private set; }
        
        public TrackRunner(Vector3 startingPosition, TrackBake startingTrack)
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

            foreach (var e in TrackBake.Tracks)
            {
                var t = e.FindClosest(position);
                var closestPoint = e.Sample(t).position;
                var distance = (closestPoint - position).magnitude;
                
                if (distance > bestScore) continue;

                bestScore = distance;
                CurrentTrack = e;
            }
        }
        
        public void FixedUpdate(Vector3 newPosition)
        {
            if (CurrentTrack == null) FindNewTrack(newPosition);
            if (CurrentTrack == null) return;

            var newTrack = CurrentTrack;
            if (CurrentTrack.IsOffStartOfTrack(newPosition)) newTrack = CurrentTrack.GetNextTrackStart();
            if (CurrentTrack.IsOffEndOfTrack(newPosition)) newTrack = CurrentTrack.GetNextTrackEnd();
            if (newTrack != null) CurrentTrack = newTrack;

            T = CurrentTrack.FindClosest(newPosition);
            var sample = CurrentTrack.Sample(T);
            Position = sample.position;
            Direction = sample.velocity.normalized;
        }
    }
}