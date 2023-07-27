// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace FR8.Splines
// {
//     public class SplinePath : ISpline
//     {
//         public Spline.SplineProfile profile;
//         public List<Vector3> knots;
//         public bool closed;
//
//         public SplinePath(Spline.SplineProfile profile, bool closed, params Vector3[] knots)
//         {
//             this.profile = profile;
//             this.knots = new List<Vector3>(knots);
//             this.closed = closed;
//
//             ValidateKnots();
//         }
//
//         private void ValidateKnots()
//         {
//             switch (knots.Count)
//             {
//                 case 0:
//                 {
//                     for (var i = 0; i < 4; i++) knots.Add(default);
//                     break;
//                 }
//                 case 1:
//                 {
//                     for (var i = 0; i < 3; i++) knots.Add(knots[0]);
//                     break;
//                 }
//                 case 2:
//                 {
//                     knots.Insert(0, 2.0f * knots[0] - knots[1]);
//                     knots.Add(2.0f * knots[2] - knots[1]);
//                     break;
//                 }
//                 case 3:
//                 {
//                     knots.Add(2.0f * knots[2] - knots[1]);
//                     break;
//                 }
//             }
//
//             if (closed)
//             {
//                 knots.Add(knots[0]);
//             }
//         }
//
//         public T Evaluate<T>(float t, Func<Spline, float, T> callback)
//         {
//             var knotCount = closed ? knots.Count + 3 : knots.Count + 2;
//
//             t *= knotCount - 3;
//             var i0 = Mathf.FloorToInt(t);
//             if (i0 >= knotCount - 4) i0 = knotCount - 4;
//
//             var p0 = knots[i0];
//             var p1 = knots[i0 + 1];
//             var p2 = knots[i0 + 2];
//             var p3 = knots[i0 + 3];
//                                  
//             return callback(Spline.CatmullRom(p0, p1, p2, p3), t - i0);
//         }
//         
//         public Vector3 EvaluatePoint(float t) => Evaluate(t, (spline, t) => spline.EvaluatePoint(t));
//         public Vector3 EvaluateVelocity(float t) => Evaluate(t, (spline, t) => spline.EvaluateVelocity(t));
//
//         public float ClosestPoint(Vector3 point)
//         {
//             
//         }
//     }
// }