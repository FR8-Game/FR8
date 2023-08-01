using System.Collections.Generic;
using UnityEngine;

namespace FR8.Train.Track
{
    public class TrackJunction : MonoBehaviour
    {
        [SerializeField] private TrackSegment primarySegment;
        [SerializeField] private TrackSegment branchingSegment;
        [SerializeField] private float checkRadius;
        [SerializeField] private bool active;

        private HashSet<TrainCarriage> deadSet = new();

        private void Awake()
        {
            var t = primarySegment.GetClosestPoint(transform.position);
            transform.position = primarySegment.SamplePoint(t);
        }

        private void FixedUpdate()
        {
            var trains = FindObjectsOfType<TrainCarriage>();

            foreach (var train in trains)
            {
                if (deadSet.Contains(train)) continue;
                if ((train.transform.position - transform.position).magnitude > checkRadius) continue;
                
                SwitchTrack(train);
                deadSet.Add(train);
            }

            var lastDeadSet = new HashSet<TrainCarriage>(deadSet);
            
            foreach (var train in lastDeadSet)
            {
                if ((train.transform.position - transform.position).magnitude < checkRadius) continue;

                deadSet.Remove(train);
            }
        }

        private void SwitchTrack(TrainCarriage train)
        {
            var segment = train.Segment;
            if (segment != primarySegment && segment != branchingSegment) return;

            if (segment == branchingSegment)
            {
                train.Segment = primarySegment;
                return;
            }
            
            if (!active) return;
            
            var vector = train.transform.position - transform.position;
            var dot = Vector3.Dot(vector.normalized, transform.forward);
            if (dot > 0.0f) return;

            train.Segment = branchingSegment;
        }

        private void OnDrawGizmos()
        {
            if (!primarySegment) return;

            var t = primarySegment.GetClosestPoint(transform.position);
            Gizmos.DrawLine(transform.position, primarySegment.SamplePoint(t));

            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;

            var length = 5.0f;
            var width = 0.1f;
            
            Gizmos.DrawCube(Vector3.zero, new Vector3(2.0f * length, width, width));
            Gizmos.DrawCube(Vector3.back * length * 0.5f, new Vector3(width, width, length));
        }
    }
}