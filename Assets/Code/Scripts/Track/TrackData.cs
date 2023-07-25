using System.Collections.Generic;
using UnityEngine;

namespace FR8.Track
{
    [CreateAssetMenu(menuName = "Config/Track Data")]
    public class TrackData : ScriptableObject
    {
        public const float MergeDistance = 4.0f;

        public List<TrackPoint> AllPoints { get; } = new();

        private void OnEnable()
        {
            BakeTrack();
        }

        public void BakeTrack()
        {
            AllPoints.AddRange(FindObjectsOfType<TrackPoint>());

            foreach (var a in AllPoints)
            {
                foreach (var b in a.Connections)
                {
                    if (b.Connections.Contains(a)) continue;
                    b.Connections.Add(a);
                }
            }
        }
    }
}