using FR8Runtime.Train;
using FR8Runtime.Train.Track;
using UnityEngine;

namespace FR8Runtime.Contracts
{
    public class TrackSection : MonoBehaviour, INameplateProvider
    {
        [SerializeField] private TrackSegment track;
        [SerializeField] private GameObject signPrefab;
        [Range(0.0f, 1.0f)] [SerializeField] private float size = 1.0f;

        public float Center => track.GetClosestPoint(transform.position);
        public float SectionStart => Mathf.Clamp01(Center - Mathf.Pow(size, 2.0f) * 0.5f);
        public float SectionEnd => Mathf.Clamp01(Center + Mathf.Pow(size, 2.0f) * 0.5f);
        public string Name => name;
        public TrackSegment Track => track;

        private void Start()
        {
            OnValidate();

            instanceSign(0);
            instanceSign(1);

            void instanceSign(int childIndex)
            {
                if (!signPrefab) return;
                
                var instance = Instantiate(signPrefab, transform.GetChild(childIndex));
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
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
            
            Transform child;
            while (transform.childCount < 2)
            {
                child = new GameObject("Handle").transform;
                child.SetParent(transform);
            }

            setChild(0, "Start", SectionStart, true);
            setChild(1, "End", SectionEnd, false);
            
            void setChild(int index, string name, float t, bool flip)
            {
                child = transform.GetChild(index);
                child.name = name;
                child.transform.position = track.SamplePoint(t);
                child.transform.rotation = Quaternion.LookRotation(track.SampleTangent(t) * (flip ? -1.0f : 1.0f), Vector3.up);
            }
        }

        private void OnDrawGizmos()
        {
            if (!track) return;

            drawEndCap(SectionStart);
            drawEndCap(Center, 0.4f);
            drawEndCap(SectionEnd);

            Gizmos.matrix = Matrix4x4.identity;
            var step = 1.0f / 100;
            for (var p = 0.0f; p < 1.0f - step; p += step)
            {
                Gizmos.DrawLine(track.SamplePoint(Mathf.Lerp(SectionStart, SectionEnd, p)), track.SamplePoint(Mathf.Lerp(SectionStart, SectionEnd, p + step)));
            }
            
            Gizmos.DrawLine(transform.position, track.SamplePoint(Center));

            void drawEndCap(float t, float scale = 1.0f)
            {
                var position = track.SamplePoint(t);
                var tangent = track.SampleTangent(t);

                var matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(tangent, Vector3.up), Vector3.one);
                Gizmos.matrix = matrix;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 1.0f, 0.0f) * 7.0f * scale);
            }
        }

        public void SnapToSpline()
        {
            var t = Center;

            transform.position = track.SamplePoint(t);
            transform.rotation = Quaternion.LookRotation(track.SampleTangent(t), Vector3.up);
        }
    }
}