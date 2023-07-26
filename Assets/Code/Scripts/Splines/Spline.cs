using UnityEngine;

namespace FR8.Splines
{
    public class Spline : ISpline
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
            var v0 = new Vector4(1.0f, t, t * t, t * t * t) * constant;
            var v1 = matrix * v0;
            
            return p0 * v1.x + p1 * v1.y + p2 * v1.z + p3 * v1.w;
        }

        public Vector3 EvaluateVelocity(float t)
        {
            var v0 = new Vector4(1.0f, t, t * t, t * t * t) * constant;
            var v1 = matrix * v0;
            
            return p0 * v1.x + p1 * v1.y + p2 * v1.z + p3 * v1.w;
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

        public static readonly SplineProfile BSpline = CreateSplineProfile(new Matrix4x4
        (
            new Vector4( 1.0f,  4.0f,  1.0f,  0.0f),
            new Vector4(-3.0f,  0.0f,  3.0f,  0.0f),
            new Vector4( 3.0f, -6.0f,  3.0f,  0.0f),
            new Vector4(-1.0f,  3.0f, -3.0f,  1.0f)
        ), 1.0f / 6.0f);
        
        public static readonly SplineProfile CatmullRom = CreateSplineProfile(new Matrix4x4
        (
            new Vector4( 0.0f,  2.0f,  0.0f,  0.0f),
            new Vector4(-1.0f,  0.0f,  1.0f,  0.0f),
            new Vector4( 2.0f, -5.0f,  4.0f, -1.0f),
            new Vector4(-1.0f,  3.0f, -3.0f,  1.0f)
        ), 0.5f);
    }
}