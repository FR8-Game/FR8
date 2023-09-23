using FR8Runtime.Train;
using FR8Runtime.Train.Track;
using UnityEngine;

namespace FR8Runtime.Contracts
{
    public class TrackSection : MonoBehaviour, INameplateProvider
    {
        [SerializeField] private TrackSegment track;
        [SerializeField] private GameObject signPrefab;
        [SerializeField] public Vector3 endPosition;
        [SerializeField] public Quaternion endRotation = Quaternion.identity;

        private Vector3 lastStartPosition;
        private Vector3 lastEndPosition;

        public float SectionStart { get; private set; }
        public float SectionEnd { get; private set; }
        public string Name => name;
        public TrackSegment Track => track;
        
        private void Start()
        {
            OnValidate();
            
            instanceSign(SectionStart, -1.0f);
            instanceSign(SectionEnd, 1.0f);

            void instanceSign(float t, float facing)
            {
                if (!signPrefab) return;
                
                var instance = Instantiate(signPrefab, transform).transform;
                instance.position = track.SamplePoint(t);
                instance.rotation = Quaternion.LookRotation(track.SampleTangent(t) * facing, Vector3.up);
            }
        }

        public bool Contains(Vector3 point)
        {
            var t = track.GetClosestPoint(point);
            return t > SectionStart && t < SectionEnd;
        }
        
        private void OnValidate()
        {
            if (!track) return;

            UpdateEnds();
        }

        private void OnDrawGizmos()
        {
            if (!track) return;
            Gizmos.color = Color.yellow;

            drawEndCap(SectionStart, transform.position);
            drawEndCap(SectionEnd, endPosition);

            var step = 1.0f / 100;
            for (var p = 0.0f; p < 1.0f - step; p += step)
            {
                Gizmos.DrawLine(track.SamplePoint(Mathf.Lerp(SectionStart, SectionEnd, p)), track.SamplePoint(Mathf.Lerp(SectionStart, SectionEnd, p + step)));
            }
            
            HBCore.Utility.GizmoUtility.Label((transform.position + endPosition) / 2.0f + Vector3.up * 10.0f, name);

            void drawEndCap(float t, Vector3 from, float scale = 1.0f)
            {
                var position = track.SamplePoint(t);
                var tangent = track.SampleTangent(t);

                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawLine(position, from);
                
                var matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(tangent, Vector3.up), Vector3.one);
                Gizmos.matrix = matrix;

                Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 1.0f, 0.0f) * 7.0f * scale);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }

        public void SnapToSpline()
        {
            GetTrackIfMissing();

            var startPosition = transform.position;

            SnapToSpline(ref startPosition, out var startRotation);
            SnapToSpline(ref endPosition, out endRotation);

            transform.position = startPosition;
            transform.rotation = startRotation;
        }

        private void SnapToSpline(ref Vector3 position, out Quaternion rotation)
        {
            var t = track.GetClosestPoint(position);

            var point = track.SamplePoint(t);
            var tangent = track.SampleTangent(t);

            var vec = point - position;
            var offset = vec - tangent * Vector3.Dot(vec, tangent);
            
            position += offset;
            rotation = Quaternion.LookRotation(track.SampleTangent(t), Vector3.up);
        }

        public void GetTrackIfMissing()
        {
            if (!track) track = TrackUtility.FindClosestSegment(transform.position).Item1;
        }

        public void UpdateEndsIfDirty()
        {
            var startPosition = transform.position;
            if (startPosition == lastStartPosition && endPosition == lastEndPosition) return;

            UpdateEnds();
            
            lastStartPosition = startPosition;
            lastEndPosition = endPosition;
        }
        
        public void UpdateEnds()
        {
            GetTrackIfMissing();

            SectionStart = track.GetClosestPoint(transform.position);
            SectionEnd = track.GetClosestPoint(endPosition);
        }

        private void Reset()
        {
            endPosition = transform.position + transform.forward * 5.0f;
            endRotation = transform.rotation;
        }
    }
}