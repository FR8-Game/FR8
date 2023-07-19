using UnityEngine;

namespace FR8.Pathing
{
    public class Curve
    {
        private Handle[] points = { };

        public Curve()
        {
            points = new Handle[0];
        }

        public Curve(Transform pathParent)
        {
            Bake(pathParent);
        }

        public void Bake(Transform pathParent)
        {
            points = new Handle[pathParent.childCount];
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = new Handle(pathParent.GetChild(i));
            }
        }

        public Vector3 Sample(float p)
        {
            var i = Mathf.FloorToInt(p * points.Length);
            var j = i + 1;

            if (i >= points.Length) return default;
            if (j >= points.Length) return points[i].position;

            var t = p * points.Length - i;

            return Interpolate(points[i], points[j], t);
        }

        public Vector3 Interpolate(Handle a, Handle b, float t)
        {
            if (a == null) return default;
            if (b == null) return default;
            
            var l = (b.position - a.position).magnitude;

            return Interpolate(new[]
            {
                a.position,
                a.position + a.handleVector * l / 3.0f,
                b.position - b.handleVector * l / 3.0f,
                b.position,
            }, t);
        }

        public static Vector3 Interpolate(Vector3[] points, float t)
        {
            while (true)
            {
                var res = new Vector3[points.Length - 1];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i] = Vector3.Lerp(points[i], points[i + 1], t);
                }

                if (res.Length == 1) return res[0];
                points = res;
            }
        }

        public Vector3 GetClosestPointOnCurve(Vector3 point, int sampleResolution = 100)
        {
            var best = 0.0f;
            var bestDistance = distance(best);

            var step = 1.0f / sampleResolution;

            for (var p = 0.0f; p < 1.0f; p += step)
            {
                var d = distance(p);
                if (d > bestDistance) continue;

                best = p;
                bestDistance = d;
            }

            return Sample(best);

            float distance(float p) => (Sample(p) - point).magnitude;
        }

        public class Handle
        {
            public Vector3 position;
            public Vector3 handleVector;

            public Handle(Vector3 position, Vector3 handleVector)
            {
                this.position = position;
                this.handleVector = handleVector;
            }

            public Handle(Transform transform)
            {
                position = transform.position;
                handleVector = transform.forward.normalized * transform.localScale.z;
            }
        }
    }
}