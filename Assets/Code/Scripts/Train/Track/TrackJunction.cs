using System;
using FR8.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8.Train.Track
{
    public class TrackJunction : MonoBehaviour, IInteractable
    {
        [SerializeField] private TrackSegment segment;
        [SerializeField] private ConnectionEnd connectionEnd;
        [SerializeField] private Transform primaryIndicator;
        [SerializeField] private Transform secondaryIndicator;
        [SerializeField] private Utility.DampedSpring animationSpring;
        [SerializeField] private bool flip;
        [SerializeField] private bool testActive;

        private bool state;
        
        public bool CanInteract => true;
        public string DisplayName => "Train Signal";
        public string DisplayValue => state ? "Engaged" : "Disengaged";
        public bool OverrideInteractDistance => true;
        public float InteractDistance => float.MaxValue;

        private void OnValidate()
        {
            if (segment)
            {
                var t = connectionEnd == ConnectionEnd.Start ? 0.0f : 1.0f;
                transform.position = segment.SamplePoint(t);
                var tangent = segment.SampleTangent(t);
                transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
            }
            
            Animate(testActive ? 1.0f : 0.0f);
        }

        private void FixedUpdate()
        {
            var connection = connectionEnd switch
            {
                ConnectionEnd.Start => segment.StartConnection,
                ConnectionEnd.End => segment.EndConnection,
                _ => throw new ArgumentOutOfRangeException()
            };

            state = connection.connectionActive;
            animationSpring.Target(state ? 1.0f : 0.0f).Iterate(Time.deltaTime);

            Animate(animationSpring.currentPosition);
        }

        private void Animate(float t)
        {
            if (flip) t = 1.0f - t;

            if (primaryIndicator) primaryIndicator.transform.localRotation = Quaternion.Euler(0.0f, t * 90.0f, 0.0f);
            if (secondaryIndicator) secondaryIndicator.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, t * -180.0f);
        }

        private enum ConnectionEnd
        {
            Start,
            End
        }
        
        public void Nudge(int direction) {  }

        public void BeginDrag(Ray ray)
        {
            var connection = connectionEnd switch
            {
                ConnectionEnd.Start => segment.StartConnection,
                ConnectionEnd.End => segment.EndConnection,
                _ => throw new ArgumentOutOfRangeException()
            };
            connection.connectionActive = !connection.connectionActive;
        }

        public void ContinueDrag(Ray ray) { }
    }
}