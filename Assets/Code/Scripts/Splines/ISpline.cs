﻿
using UnityEngine;

namespace FR8.Splines
{
    public interface ISpline
    {
        Vector3 EvaluatePoint(float t);
        Vector3 EvaluateVelocity(float t);
        float ClosestPoint(Vector3 point);
    }
}