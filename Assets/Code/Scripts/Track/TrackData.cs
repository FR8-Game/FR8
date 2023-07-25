using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FR8.Track
{
    [CreateAssetMenu(menuName = "Config/Track Data")]
    public class TrackData : ScriptableObject
    { 
        public List<Point> AllPoints { get; } = new();

        private void OnEnable()
        {
            BakeTrack();
        }

        public void BakeTrack()
        {
            var segments = new List<TrackSegment>(FindObjectsOfType<TrackSegment>());

            AllPoints.Clear();
            
            foreach (var segment in segments)
            {
                segment.FindKnots();
                Point lastPoint = null;
                foreach (var knot in segment.Knots)
                {
                    var point = new Point(knot.position);

                    if (lastPoint != null)
                    {
                        point.previousPoints.Add(lastPoint);
                        lastPoint.nextPoints.Add(point);
                    }
                    AllPoints.Add(point);
                    lastPoint = point;
                }
            }

            var mergedPoints = new List<(Point, Point)>();
            foreach (var a in AllPoints)
            {
                foreach (var b in AllPoints)
                {
                    if (a == b) continue;
                    
                    var dist = (b.position - a.position).sqrMagnitude;
                    if (dist > 1.0f * 1.0f) continue;

                    mergedPoints.Add((b, a));
                    
                    break;
                }
            }

            foreach (var merge in mergedPoints)
            {
                AllPoints.Remove(merge.Item1);
                // TODO this shit
            }

            Debug.Log(AllPoints.Count);
        }

        public class Point
        {
            public Vector3 position;

            public List<Point> previousPoints = new();
            public List<Point> nextPoints = new();

            public int previousPointIndex;
            public int nextPointIndex;

            public Point(Vector3 position)
            {
                this.position = position;
            }
        }
    }
}