using UnityEngine;

namespace FR8
{
    [System.Serializable]
    public class Rope
    {
        private Vector3 velocity;

        public Vector3 Start { get; private set; }
        public Vector3 Mid { get; private set; }
        public Vector3 End { get; private set; }

        public void Update(Vector3 start, Vector3 end, float length)
        {
            var force = Physics.gravity;
            
            this.Start = start;
            this.End = end;
            
            var d0 = (end - start).magnitude;
            var d1 = Mathf.Sqrt(length * length - d0 * d0);

            var midPoint = (start + end) / 2.0f;

            if ((Mid - midPoint).magnitude > d1)
            {
                var dir = (Mid - midPoint).normalized;
                Mid = midPoint + dir * d1;
                velocity -= dir * Mathf.Max(Vector3.Dot(dir, velocity), 0.0f);
            }
            
            Mid += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
        }

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