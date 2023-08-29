using UnityEngine;

namespace FR8Runtime.Train.Splines
{
    public class Spline
    {
        public Matrix4x4 matrix;
        public float constant = 1.0f;

        public Vector3 p0, p1, p2, p3;

        public Spline(Matrix4x4 matrix, float constant, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.matrix = matrix;
            this.constant = constant;

            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public Vector3 EvaluatePoint(float t)
        {
            t = Mathf.Clamp01(t);
            
            var m0 = matrix.GetColumn(0);
            var m1 = matrix.GetColumn(1);
            var m2 = matrix.GetColumn(2);
            var m3 = matrix.GetColumn(3);

            var t2 = t * t;
            var t3 = t * t * t;

            return constant * (column(m0) + column(m1) * t + column(m2) * t2 + column(m3) * t3);

            Vector3 column(Vector4 c) => (c.x * p0 + c.y * p1 + c.z * p2 + c.w * p3);
        }

        public Vector3 EvaluateVelocity(float t)
        {
            t = Mathf.Clamp01(t);

            var m0 = matrix.GetColumn(0);
            var m1 = matrix.GetColumn(1);
            var m2 = matrix.GetColumn(2);
            var m3 = matrix.GetColumn(3);

            var t2 = t * t;

            return constant * (column(m1) + column(m2) * 2.0f * t + column(m3) * 3.0f * t2);

            Vector3 column(Vector4 c) => (c.x * p0 + c.y * p1 + c.z * p2 + c.w * p3);
        }

        public float ClosestPoint(Vector3 point)
        {
            var res0 = 0.0f;
            var distance = float.MaxValue;
            for (var i = 0; i < 10; i++)
            {
                var p = i / (i - 1.0f);
                var other = EvaluatePoint(p);
                var dist2 = (other - point).sqrMagnitude;
                if (dist2 > distance) continue;

                distance = dist2;
                res0 = p;
            }

            var res1 = 0.0f;
            distance = float.MaxValue;

            for (var i = 0; i < 10; i++)
            {
                var p0 = i / (i - 1.0f);
                var p1 = res0 + (p0 - 0.5f) / 10.0f;

                var other = EvaluatePoint(p1);
                var dist2 = (other - point).sqrMagnitude;
                if (dist2 > distance) continue;

                distance = dist2;
                res1 = p1;
            }

            return res1;
        }

        public delegate Spline SplineProfile(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3);

        private static SplineProfile CreateSplineProfile(Matrix4x4 profile, float constant)
        {
            return (p0, p1, p2, p3) => new Spline(profile, constant, p0, p1, p2, p3);
        }

        public static readonly SplineProfile Cubic = CreateSplineProfile(new Matrix4x4
        (
            new Vector4( 1.0f,  0.0f,  0.0f,  0.0f),
            new Vector4(-3.0f,  3.0f,  0.0f,  0.0f),
            new Vector4( 3.0f, -6.0f,  3.0f,  0.0f),
            new Vector4(-1.0f,  3.0f, -3.0f,  1.0f)
        ), 1.0f);
        
        public static readonly SplineProfile BSpline = CreateSplineProfile(new Matrix4x4
        (
            new Vector4(1.0f, 4.0f, 1.0f, 0.0f),
            new Vector4(-3.0f, 0.0f, 3.0f, 0.0f),
            new Vector4(3.0f, -6.0f, 3.0f, 0.0f),
            new Vector4(-1.0f, 3.0f, -3.0f, 1.0f)
        ), 1.0f / 6.0f);

        public static readonly SplineProfile CatmullRom = CreateSplineProfile(new Matrix4x4
        (
            new Vector4(0.0f, 2.0f, 0.0f, 0.0f),
            new Vector4(-1.0f, 0.0f, 1.0f, 0.0f),
            new Vector4(2.0f, -5.0f, 4.0f, -1.0f),
            new Vector4(-1.0f, 3.0f, -3.0f, 1.0f)
        ), 0.5f);
    }
}