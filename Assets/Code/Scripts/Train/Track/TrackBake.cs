using System;
using System.Collections.Generic;
using UnityEngine;

namespace FR8Runtime.Train.Track
{
    public class TrackBake
    {
        public TrackSegment parent;
        public List<TrackPoint> samples = new();
        public List<TrackBake> startConnections = new();
        public int activeStartConnectionIndex;
        public List<TrackBake> endConnections = new();
        public int activeEndConnectionIndex;

        public int Count => samples.Count;

        public static readonly List<TrackBake> Tracks = new();

        public TrackBake(TrackSegment parent)
        {
            this.parent = parent;
        }

        public TrackPoint Sample(float t0)
        {
            t0 = Mathf.Clamp01(t0);

            var t1 = t0 * Count;
            var i0 = Mathf.FloorToInt(t1);
            var i1 = i0 + 1;

            if (i1 >= Count) return samples[^1];

            var a = samples[i0];
            var b = samples[i1];

            return TrackPoint.Lerp(a, b, Mathf.InverseLerp(i0, i1, t1));
        }

        public float FindClosest(Vector3 point, float heuristic = 0.0f)
        {
            var bestDistance = float.MaxValue;
            var bestT = 0.0f;
            
            for (var i0 = 0; i0 < Count - 1; i0++)
            {
                var i1 = i0;

                var a = samples[i1];
                var b = samples[i1 + 1];
                var t0 = ClosestPointOnLine(a.position, b.position, point);
                var t1 = Mathf.Lerp(i1, i1 + 1, t0) / Count;
                var distance = (Sample(t1).position - point).magnitude;
                
                if (distance > bestDistance) continue;
                bestDistance = distance;
                bestT = t1;
            }

            return bestT;
        }

        public bool IsOffStartOfTrack(Vector3 position) => IsOffTrack(position, 0, 1);
        public bool IsOffEndOfTrack(Vector3 position) => IsOffTrack(position, FromEnd(0), FromEnd(1));
        private bool IsOffTrack(Vector3 position, int startPoint, int endPoint)
        {
            var a = samples[startPoint].position;
            var b = samples[endPoint].position;
            var direction = b - a;

            return Vector3.Dot(position - a, direction) < 0.0f;
        }

        public TrackBake GetNextTrackStart() => activeStartConnectionIndex >= 0 && activeStartConnectionIndex < endConnections.Count - 1 ? startConnections[activeStartConnectionIndex] : null;
        public TrackBake GetNextTrackEnd() => activeEndConnectionIndex >= 0 && activeEndConnectionIndex < endConnections.Count - 1 ? endConnections[activeEndConnectionIndex] : null;

        private int FromEnd(int i) => Count - 1 - i;
        
        private static float ClosestPointOnLine(Vector3 start, Vector3 end, Vector3 point)
        {
            var localPoint = point - start;
            var lineDirection = (end - start).normalized;
            var lineLength = (end - start).magnitude;
            
            var dot = Vector3.Dot(localPoint, lineDirection);
            dot = Mathf.Clamp01(dot / lineLength);
            return dot;
        }

        public static void Add(TrackSegment segment)
        {
            var bakeData = (TrackBake)null;
            foreach (var e in Tracks)
            {
                if (e.parent != segment) continue;
                bakeData = e;
                break;
            }

            if (bakeData == null)
            {
                bakeData = new TrackBake(segment);
                Tracks.Add(bakeData);
            }

            bakeData.samples.Clear();

            var start = bakeData.Count;
            for (var i = 0; i < segment.Resolution; i++)
            {
                var t = i / (segment.Resolution - 1.0f);
                bakeData.samples.Add(new TrackPoint()
                {
                    position = segment.SamplePoint(t),
                    velocity = segment.SampleVelocity(t),
                });
            }

            CheckForNewConnections(bakeData);
        }

        private static void CheckForNewConnections(TrackBake newSegment)
        {
            foreach (var other in Tracks)
            {
                if (other == newSegment) continue;
            }
        }

        public static void Remove(TrackSegment segment) => Tracks.RemoveAll(e => e.parent == segment);
    }

    public struct TrackPoint
    {
        public Vector3 position;
        public Vector3 velocity;

        public static TrackPoint Lerp(TrackPoint a, TrackPoint b, float t)
        {
            return new TrackPoint()
            {
                position = Vector3.Lerp(a.position, b.position, t),
                velocity = Vector3.Lerp(a.velocity, b.velocity, t).normalized * Mathf.Lerp(a.velocity.magnitude, b.velocity.magnitude, t),
            };
        }
    }
}