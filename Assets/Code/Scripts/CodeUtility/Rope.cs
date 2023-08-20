using UnityEngine;

namespace FR8
{
    [System.Serializable]
    public class Rope
    {
        private Vector3 start;
        private Vector3 end;
        
        private Vector3 hangingPoint;
        private Vector3 velocity;

        public void Update(Vector3 start, Vector3 end, float length)
        {
            var force = Physics.gravity;
            
            this.start = start;
            this.end = end;
            
            var d0 = (end - start).magnitude;
            var d1 = Mathf.Sqrt(length * length - d0 * d0);

            var midPoint = (start + end) / 2.0f;

            if ((hangingPoint - midPoint).magnitude > d1)
            {
                var dir = (hangingPoint - midPoint).normalized;
                hangingPoint = midPoint + dir * d1;
                velocity -= dir * Mathf.Max(Vector3.Dot(dir, velocity), 0.0f);
            }
            
            hangingPoint += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
        }

        public Vector3 Sample(float percent)
        {
            var ab = Vector3.Lerp(start, hangingPoint, percent);
            var bc = Vector3.Lerp(hangingPoint, end, percent);
            return Vector3.Lerp(ab, bc, percent);
        }
        
        public void DrawGizmos()
        {
            var midPoint = (start + end) / 2.0f;
            
            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(midPoint, hangingPoint);
            
            Gizmos.DrawSphere(start, 0.1f);
            Gizmos.DrawSphere(end, 0.1f);
            Gizmos.DrawSphere(midPoint, 0.1f);
            Gizmos.DrawSphere(hangingPoint, 0.1f);

            for (var i = 0; i < 100; i++)
            {
                var p0 = i / 100.0f;
                var p1 = (i + 1) / 100.0f;
                
                Gizmos.DrawLine(Sample(p0), Sample(p1));
            }
        }
    }
}