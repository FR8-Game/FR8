using UnityEngine;

namespace FR8.Splines
{
    public class Spline
    {
        public Matrix4x4 matrix;
        public float constant = 1.0f;

        public Spline(Matrix4x4 matrix, float constant)
        {
            this.matrix = matrix;
            this.constant = constant;
        }

        public Vector3 Evaluate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var v0 = new Vector4(1.0f, t, t * t, t * t * t) * constant;
            var v1 = matrix * v0;
            
            return p0 * v1.x + p1 * v1.y + p2 * v1.z + p3 * v1.w;
        }

        public static readonly Spline BSpline = new(new Matrix4x4
        (
            new Vector4( 1.0f,  4.0f,  1.0f,  0.0f),
            new Vector4(-3.0f,  0.0f,  3.0f,  0.0f),
            new Vector4( 3.0f, -6.0f,  3.0f,  0.0f),
            new Vector4(-1.0f,  3.0f, -3.0f,  1.0f)
        ), 1.0f / 6.0f);
        
        public static readonly Spline CatmullRom = new(new Matrix4x4
        (
            new Vector4( 0.0f,  2.0f,  0.0f,  0.0f),
            new Vector4(-1.0f,  0.0f,  1.0f,  0.0f),
            new Vector4( 2.0f, -5.0f,  4.0f, -1.0f),
            new Vector4(-1.0f,  3.0f, -3.0f,  1.0f)
        ), 0.5f);
    }
}