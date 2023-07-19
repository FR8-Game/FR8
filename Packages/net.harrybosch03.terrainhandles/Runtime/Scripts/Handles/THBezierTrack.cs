using FR8.Pathing;
using UnityEngine;

namespace TerrainHandles.Handles
{
    [RequireComponent(typeof(PathCurve))]
    public class THBezierTrack : TerrainHandle
    {
        [SerializeField] private float trackRadius;
        [SerializeField] private int curveResolution;
        [SerializeField] private float spread;

        private Vector3[] curvePoints;

        public override void Prepare()
        {
            base.Prepare();
            var pathCurve = GetComponent<PathCurve>();
            var curve = pathCurve.GetCurve();

            curvePoints = new Vector3[curveResolution];
            for (var i = 0; i < curvePoints.Length; i++)
            {
                var p = i / (float)curvePoints.Length;
                curvePoints[i] = curve.Sample(p);
            }
        }

        public override float Apply(float w, Vector3 point, TerrainData data)
        {
            var closestPoint = ClosestPoint(point);

            var vector = closestPoint - point;
            var distance = trackRadius - new Vector3(vector.x, 0.0f, vector.z).magnitude + Mathf.Abs(vector.y) * spread;

            return distance > 0.0f ? vector.y : w;
        }

        public override bool IsChunkAffected(Chunk chunk)
        {
            
        }

        private Vector3 ClosestPoint(Vector3 point)
        {
            var best = 0;
            var bestDist = (curvePoints[0] - point).sqrMagnitude;

            for (var i = 1; i < curvePoints.Length; i++)
            {
                var dist = (curvePoints[i] - point).sqrMagnitude;
                if (dist > bestDist) continue;
                best = i;
                bestDist = dist;
            }

            return curvePoints[best];
        }
    }
}