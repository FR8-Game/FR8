using FR8Runtime.Train.Track;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] private TrackSegment target;
    [SerializeField][Range(0.0f, 1.0f)] private float t;

    private void OnDrawGizmos()
    {
        if (!target) return;
        
        var position = target.SamplePoint(t);
        var velocity = target.SampleVelocity(t);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(position, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(position, velocity);

        var closest = target.GetClosestPoint(transform.position, true);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, target.SamplePoint(closest));
    }
}
