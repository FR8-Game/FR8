using System;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FR8Runtime.Tools
{
    public class DiscSampler : MonoBehaviour
    {
        [SerializeField] private float range = 20.0f;
        [SerializeField] private int samples = 200;
        [SerializeField] private float maxDiscRadius = 2.0f;
        [SerializeField] private float minDiscRadius = 1.0f;
        [SerializeField] private int seed = 0;
        [SerializeField] private float searchDistance;

        private List<Disc> points;

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            BakePoints();

            Handles.color = Color.green;
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360.0f, range);
            Handles.DrawWireArc(transform.position + Vector3.down * searchDistance, Vector3.up, Vector3.forward, 360.0f, range);
            
            drawCircleEdge(Vector3.left);
            drawCircleEdge(Vector3.right);
            drawCircleEdge(Vector3.forward);
            drawCircleEdge(Vector3.back);
            
            Gizmos.color = Color.white;
            foreach (var p in points)
            {
                Handles.DrawWireArc(p.position, Vector3.up, Vector3.forward, 360.0f, p.radius);
                Gizmos.DrawSphere(p.position, 0.1f);
            }

            foreach (var p in points)
            {
                Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, Handles.color.a * 0.8f);
                Handles.DrawLine(p.position, new Vector3(p.position.x, transform.position.y, p.position.z));
            }

            void drawCircleEdge(Vector3 direction)
            {
                Handles.DrawLine(transform.position + direction * range, transform.position + Vector3.down * searchDistance + direction * range);
            }
#endif
        }

        private void BakePoints()
        {
            var grid = new Dictionary<Vector2Int, List<Disc>>();
            var deadPoints = new List<int>();
            var random = new System.Random(seed);

            points = new List<Disc>();
            
            registerPoint(new Disc(transform.position, rand(minDiscRadius, maxDiscRadius)));

            var i = 0;
            var index = 0;
            while (points.Count < samples && i < 50000)
            {
                i++;

                var a = rand(0.0f, 2.0f * Mathf.PI);
                index = (index + 1) % points.Count;
                var other = points[index];
                var radius = rand(minDiscRadius, maxDiscRadius);
                var newPoint = new Disc(other.position + new Vector3(Mathf.Cos(a), 0.0f, Mathf.Sin(a)) * (radius + other.radius), radius);
                
                if (deadPoints[index] > 24) continue;
                
                if (!canUsePoint(newPoint))
                {
                    deadPoints[index]++;
                    continue;
                }

                registerPoint(newPoint);
            }

            PlacePointsOnSurface();
            
            float rand(float lower = 0.0f, float upper = 1.0f) => (float)random.NextDouble() * (upper - lower) + lower;
            Vector2Int getKey(Disc disc) => Vector2Int.RoundToInt(new Vector2(disc.position.x, disc.position.z) / maxDiscRadius);

            List<Disc> getClosestPoints(Disc point)
            {
                var res = new List<Disc>();
                var key = getKey(point);

                for (var x = -2; x < 3; x++)
                for (var y = -2; y < 3; y++)
                {
                    var k2 = new Vector2Int(key.x + x, key.y + y);
                    if (grid.ContainsKey(k2)) res.AddRange(grid[k2]);
                }

                return res;
            }

            void registerPoint(Disc point)
            {
                points.Add(point);
                deadPoints.Add(0);

                var key = getKey(point);
                if (!grid.ContainsKey(key)) grid.Add(key, new List<Disc>());
                grid[key].Add(point);
            }

            bool canUsePoint(Disc point)
            {
                var range = this.range - point.radius;
                if ((point.position - transform.position).sqrMagnitude > range * range) return false;

                var closePoints = getClosestPoints(point);
                foreach (var other in closePoints)
                {
                    var d2 = (point.position - other.position).sqrMagnitude;
                    var dist = point.radius + other.radius;
                    if (d2 > dist * dist) continue;
                    return false;
                }

                return true;
            }
        }

        private void PlacePointsOnSurface()
        {
            var newPoints = new List<Disc>();
            
            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];

                var ray = new Ray(point.position, Vector3.down);
                if (!Physics.Raycast(ray, out var hit, searchDistance)) continue;
                point.position = hit.point;
                
                newPoints.Add(point);
            }
            
            points.Clear();
            points.AddRange(newPoints);
        }

        private void OnValidate()
        {
            samples = Mathf.Max(1, samples);
            maxDiscRadius = Mathf.Max(0.0f, maxDiscRadius);
            range = Mathf.Max(0.0f, range);
        }

        [Serializable]
        public struct Disc
        {
            public Vector3 position;
            public float radius;

            public Disc(Vector3 position, float radius)
            {
                this.position = position;
                this.radius = radius;
            }
        }
    }
}