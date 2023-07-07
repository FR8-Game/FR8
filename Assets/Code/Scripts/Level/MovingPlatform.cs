using UnityEngine;

namespace FR8.Level
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private AnimationCurve easingCurve;
        [SerializeField] private float duration;
        [SerializeField] private float postDelay;
        
        private Transform path;
        private Rigidbody platform;
        private int index;
        private float clock;
        
        private void Awake()
        {
            Configure();
        }

        private void OnValidate()
        {
            Configure();
        }

        private void Configure()
        {
            while (transform.childCount < 2) new GameObject("Child").transform.SetParent(transform);

            platform = transform.GetChild(0).gameObject.GetOrAddComponent<Rigidbody>();
            platform.gameObject.name = "Platform";
            platform.isKinematic = true;
            platform.useGravity = false;
            
            platform.transform.localPosition = Vector3.zero;
            platform.transform.localRotation = Quaternion.identity;
            
            path = transform.GetChild(1);
            path.name = "Path";
            path.transform.localPosition = Vector3.zero;
            path.transform.localRotation = Quaternion.identity;

            for (var i = 0; i < path.childCount; i++)
            {
                path.GetChild(i).name = $"Path Point {i}";
            }
        }

        private void FixedUpdate()
        {
            var last = PathPoint(index);
            var next = PathPoint(index + 1);

            var position = platform.position;
            var rotation = platform.rotation;
            
            if (clock < duration)
            {
                var t = easingCurve.Evaluate(clock / duration);
                position = Vector3.Lerp(last.position, next.position, t);
                rotation = Quaternion.Slerp(last.rotation, next.rotation, t);
            }
            else if (clock > duration + postDelay)
            {
                clock -= duration + postDelay;
                index++;
            }
            
            platform.MovePosition(position);
            platform.MoveRotation(rotation);
            clock += Time.deltaTime;
        }

        private Transform PathPoint(int i) => path.GetChild((i % path.childCount + path.childCount) % path.childCount);

        private void OnDrawGizmos()
        {
            DrawGizmos(new Color(1.0f, 1.0f, 0.0f, 0.2f));
        }
        
        private void OnDrawGizmosSelected()
        {
            DrawGizmos(Color.yellow);
        }

        private void DrawGizmos(Color color)
        {
            Gizmos.color = color;

            path = transform.GetChild(1);

            for (var i = 0; i < path.childCount; i++)
            {
                var a = PathPoint(i);
                var b = PathPoint(i + 1);
                
                Gizmos.DrawLine(a.position, b.position);
            }
        }
    }
}