using UnityEngine;

namespace FR8Runtime.Train.Track
{
    public class TrackDebugger : MonoBehaviour
    {
        [SerializeField] private TrackSegment segment;
        [SerializeField][Range(0.0f, 1.0f)] private float t;
        [SerializeField] private bool useClosestPoint;
        [SerializeField] private bool roundToKnot;

        private void OnDrawGizmos()
        {
            if (!segment) return;

            if (useClosestPoint) t = segment.GetClosestPoint(transform.position, true);

            var p = segment.SamplePoint(t);
            var v = segment.SampleVelocity(t);

            if (roundToKnot)
            {
                var i = segment.GetKnotIndex(t);
                p = segment[i].position;
                v = segment.KnotVelocity(i);
            }
            
            Debug.DrawLine(transform.position, p);
            Debug.DrawRay(p, v);
            Debug.DrawRay(p, -v);
        }
    }
}