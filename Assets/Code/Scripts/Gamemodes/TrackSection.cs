using FR8.Runtime.Train;
using FR8.Runtime.Train.Track;
using UnityEngine;

namespace FR8.Runtime.Gamemodes
{
    public class TrackSection : MonoBehaviour, INameplateProvider
    {
        [SerializeField] private TrackSegment track;
        [SerializeField] private GameObject signPrefab;
        [SerializeField] private GameObject zone;
        [SerializeField] private float zoneWidth;
        [SerializeField] private float zoneHeight;
        [SerializeField] public Vector3 localStartPosition;
        [SerializeField] public Quaternion localStartRotation = Quaternion.identity;
        [SerializeField] public Vector3 localEndPosition;
        [SerializeField] public Quaternion localEndRotation = Quaternion.identity;

        private Vector3 lastStartPosition;
        private Vector3 lastEndPosition;

        public Vector3 StartPosition
        {
            get => transform.TransformPoint(localStartPosition);
            set => localStartPosition = transform.InverseTransformPoint(value);
        }

        public Quaternion StartRotation
        {
            get => transform.rotation * localStartRotation;
            set => localStartRotation = Quaternion.Inverse(transform.rotation) * value;
        }

        public Vector3 EndPosition
        {
            get => transform.TransformPoint(localEndPosition);
            set => localEndPosition = transform.InverseTransformPoint(value);
        }

        public Quaternion EndRotation
        {
            get => transform.rotation * localEndRotation;
            set => localEndRotation = Quaternion.Inverse(transform.rotation) * value;
        }

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

            SetupZone();
        }

        private void SetupZone()
        {
            var find = transform.Find("Zone");
            var zone = find ? find.gameObject : null;
            if (!zone && this.zone)
            {
#if UNITY_EDITOR
                zone = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(this.zone);
#else
                zone = Instantiate(this.zone);
#endif
                zone.transform.SetParent(transform);
                zone.name = "Zone";
            }

            if (!zone) return;

            var start = StartPosition;
            var end = EndPosition;
            var center = (start + end) / 2.0f;
            var vector = end - start;
            var length = vector.magnitude;

            zone.transform.rotation = Quaternion.LookRotation(vector, Vector3.up);
            zone.transform.position = center + zone.transform.up * (zoneHeight / 2.0f - 1.0f);
            zone.transform.localScale = new Vector3(zoneWidth, zoneHeight, length);
        }

        public bool Contains(Vector3 point)
        {
            var t = track.GetClosestPoint(point, false);
            return t > SectionStart && t < SectionEnd;
        }

        private void OnValidate()
        {
            if (!track) return;

            UpdateEnds();

            SetupZone();
        }

        private void OnDrawGizmos()
        {
            if (!track) return;
            Gizmos.color = Color.yellow;

            drawEndCap(SectionStart, StartPosition);
            drawEndCap(SectionEnd, EndPosition);

            var step = 1.0f / 100;
            for (var p = 0.0f; p < 1.0f - step; p += step)
            {
                Gizmos.DrawLine(track.SamplePoint(Mathf.Lerp(SectionStart, SectionEnd, p)), track.SamplePoint(Mathf.Lerp(SectionStart, SectionEnd, p + step)));
            }

            HBCore.Utility.GizmoUtility.Label((StartPosition + EndPosition) / 2.0f + Vector3.up * 10.0f, name);

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

            var (startPosition, startRotation) = SnapToSpline(StartPosition);
            var (endPosition, endRotation) = SnapToSpline(EndPosition);

            transform.position = (startPosition + endPosition) / 2.0f;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, 0.5f);

            StartPosition = startPosition;
            StartRotation = startRotation;
            EndPosition = endPosition;
            EndRotation = endRotation;
        }

        private (Vector3, Quaternion) SnapToSpline(Vector3 position)
        {
            var t = track.GetClosestPoint(position, true);

            var point = track.SamplePoint(t);
            var tangent = track.SampleTangent(t);

            var vec = point - position;
            var offset = vec - tangent * Vector3.Dot(vec, tangent);

            position += offset;
            var rotation = Quaternion.LookRotation(track.SampleTangent(t), Vector3.up);

            return (position, rotation);
        }

        public void GetTrackIfMissing()
        {
            if (!track) track = TrackUtility.FindClosestSegment(transform.position).Item1;
        }

        public void UpdateEndsIfDirty()
        {
            if (StartPosition == lastStartPosition && EndPosition == lastEndPosition) return;

            UpdateEnds();

            lastStartPosition = StartPosition;
            lastEndPosition = EndPosition;
        }

        public void UpdateEnds()
        {
            GetTrackIfMissing();
            if (!track) return;

            SectionStart = track.GetClosestPoint(StartPosition, true);
            SectionEnd = track.GetClosestPoint(EndPosition, true);
        }

        private void Reset()
        {
            localStartPosition = Vector3.back * 5.0f;
            localStartRotation = Quaternion.identity;

            localEndPosition = Vector3.forward * 5.0f;
            localEndRotation = Quaternion.identity;
        }
    }
}