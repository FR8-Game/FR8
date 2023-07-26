using FR8.Splines;
using UnityEngine;

namespace FR8.Track
{
    [System.Serializable]
    public class TrackWalker
    {
        [SerializeField] private TrackSegment segment;

        private int segmentIndex;

        public (Spline, float) Walk(Vector3 newPosition)
        {
            var spline = TrackSegment.SplineProfile
            (
                segment.Knots[segmentIndex].position,
                segment.Knots[segmentIndex + 1].position,
                segment.Knots[segmentIndex + 2].position,
                segment.Knots[segmentIndex + 3].position
            );

            var t = spline.ClosestPoint(newPosition);

            if (t < 0.25f)
            {
                var end = segment.Knots[segmentIndex].GetComponent<TrackJunction>();
                if (!end || end.Index == -1)
                {
                    if (segmentIndex + 4 < segment.Knots.Count) segmentIndex++;
                    return Walk(newPosition);
                }
                
            }

            if (t > 0.75f)
            {
                var end = segment.Knots[segmentIndex + 3].GetComponent<TrackJunction>();
                if (!end || end.Index == -1)
                {
                    if (segmentIndex - 1 >= 0) segmentIndex--;
                    return Walk(newPosition);
                }

                segment = end.SubSegments[end.Index];
                segmentIndex = 0;
            }
            
            return (spline, t);
        }
    }
}