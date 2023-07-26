using System.Collections.Generic;
using UnityEngine;

namespace FR8.Track
{
    public class TrackJunction : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private ConnectionDirection superSegmentConnectionDirection = ConnectionDirection.Positive;

        public TrackSegment SuperSegment { get; private set; }
        public List<TrackSegment> SubSegments { get; private set; } 
        
        public List<Transform> BranchKnots { get; private set; }
        public int Index => index;

        private void Awake()
        {
            Setup();
        }

        public void Setup()
        {
            SuperSegment = transform.parent.GetComponent<TrackSegment>();

            SubSegments = new List<TrackSegment>();
            foreach (Transform child in transform)
            {
                if (!child.TryGetComponent(out TrackSegment segment)) continue;
                SubSegments.Add(segment);
            }

            BranchKnots = new List<Transform>();
            var rootIndex = SuperSegment.Knots.FindIndex(t => t == transform);
            for (var i = 0; i < 1; i++)
            {
                BranchKnots.Add(SuperSegment.Knots[rootIndex + i * (int)superSegmentConnectionDirection]);
            }
        }

        private void Reset()
        {
            Setup();
            if (SubSegments.Count > 0) return;

            var subSegment = new GameObject("Track Segment").AddComponent<TrackSegment>();
            subSegment.transform.SetParent(transform);
            subSegment.transform.position = Vector3.zero;
            subSegment.transform.rotation = Quaternion.identity;

            Setup();
        }

        public enum ConnectionDirection
        {
            Positive = 1,
            Negative = -1,
        }
    }
}
