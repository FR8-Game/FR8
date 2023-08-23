using FR8Runtime.Level;
using UnityEngine;

namespace FR8Runtime.CodeUtility
{
    [System.Serializable]
    public class Rope
    {
        [SerializeField] private float drag;
        [SerializeField] private bool useWind;
        [SerializeField] private float windScale;
        [SerializeField] private bool reset;
        
        private Vector3 velocity;
        private bool initialized;

        private WindZone[] windZones;
        
        public Vector3 Start { get; private set; }
        public Vector3 Mid { get; private set; }
        public Vector3 End { get; private set; }

        public void Update(Vector3 start, Vector3 end, float length)
        {
            if (!initialized)
            {
                windZones = Object.FindObjectsOfType<WindZone>();
                initialized = true;
            }

            if (reset)
            {
                velocity = Vector3.zero;
                Mid = (start + end) / 2.0f;
                reset = false;
            }
            
            var force = UnityEngine.Physics.gravity;

            force += CalculateWindForces();
            force -= velocity.normalized * Mathf.Min(velocity.sqrMagnitude * drag, velocity.magnitude / Time.deltaTime);
            
            Start = start;
            End = end;
            
            var d0 = (end - start).magnitude;
            var d1 = Mathf.Sqrt(length * length - d0 * d0);

            Mid += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            
            var midPoint = (start + end) / 2.0f;

            if ((Mid - midPoint).magnitude > d1)
            {
                var dir = (Mid - midPoint).normalized;
                Mid = midPoint + dir * d1;
                velocity -= dir * Mathf.Max(Vector3.Dot(dir, velocity), 0.0f);
            }
        }

        private Vector3 CalculateWindForces() => useWind ? WindSettings.GetWindForce(Mid) * windScale : Vector3.zero;

        public Vector3 Sample(float percent)
        {
            var ab = Vector3.Lerp(Start, Mid, percent);
            var bc = Vector3.Lerp(Mid, End, percent);
            return Vector3.Lerp(ab, bc, percent);
        }
        
        public void DrawGizmos()
        {
            var midPoint = (Start + End) / 2.0f;
            
            Gizmos.DrawLine(Start, End);
            Gizmos.DrawLine(midPoint, Mid);
            
            Gizmos.DrawSphere(Start, 0.1f);
            Gizmos.DrawSphere(End, 0.1f);
            Gizmos.DrawSphere(midPoint, 0.1f);
            Gizmos.DrawSphere(Mid, 0.1f);

            for (var i = 0; i < 100; i++)
            {
                var p0 = i / 100.0f;
                var p1 = (i + 1) / 100.0f;
                
                Gizmos.DrawLine(Sample(p0), Sample(p1));
            }
        }
    }
}