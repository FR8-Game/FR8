using FR8Runtime.Train.Track;
using UnityEngine;

namespace FR8Runtime.Contracts
{
    public class TrackSection : MonoBehaviour
    {
        [SerializeField] private string displayName = "Funny Track";
        [SerializeField] private TrackSegment track;
        [Range(0.0f, 1.0f)] [SerializeField] private float start = 0.0f;
        [Range(0.0f, 1.0f)] [SerializeField] private float end = 1.0f;

        private void OnValidate()
        {
            start = Mathf.Clamp(start, 0.0f, end);
            end = Mathf.Clamp(end, start, 1.0f);
        }

        private void OnDrawGizmos()
        {
            if (!track) return;

            drawEndCap(start);
            drawEndCap(end);

            Gizmos.matrix = Matrix4x4.identity;
            var step = 1.0f / 100;
            for (var p = 0.0f; p < 1.0f - step; p += step)
            {
                Gizmos.DrawLine(track.SamplePoint(Mathf.Lerp(start, end, p)), track.SamplePoint(Mathf.Lerp(start, end, p + step)));
            }


            void drawEndCap(float t)
            {
                var position = track.SamplePoint(t);
                var tangent = track.SampleTangent(t);

                var matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(tangent, Vector3.up), Vector3.one);
                Gizmos.matrix = matrix;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 1.0f, 0.0f) * 7.0f);
            }
        }
    }
}