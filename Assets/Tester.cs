using FR8Runtime.Train.Track;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField][Range(0.0f, 1.0f)] private float t;

    private void OnDrawGizmos()
    {
        if (index < 0 || index >= TrackBake.Tracks.Count) return;

        var track = TrackBake.Tracks[index];
        var sample = track.Sample(t);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(sample.position, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(sample.position, sample.velocity);

        var closest = track.FindClosest(transform.position);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, track.Sample(closest).position);
    }
}
