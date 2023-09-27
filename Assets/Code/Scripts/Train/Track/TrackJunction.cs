using System;
using System.Collections.Generic;
using FR8Runtime.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8Runtime.Train.Track
{
    public class TrackJunction : MonoBehaviour, IInteractable
    {
        [SerializeField] private TrackSegment segment;
        [SerializeField] private Transform primaryIndicator;
        [SerializeField] private Transform secondaryIndicator;
        [SerializeField] private CodeUtility.DampedSpring animationSpring;
        [SerializeField] private bool flip;
        [SerializeField] private bool testActive;
        
        [Space]
        [SerializeField] private ConnectionEnd connectionEnd;

        private bool state;

        public bool CanInteract => true;
        public string DisplayName => "Train Signal";
        public string DisplayValue => state ? "Engaged" : "Disengaged";
        public bool OverrideInteractDistance => true;
        public float InteractDistance => float.MaxValue;
        public IEnumerable<Renderer> Visuals { get; private set; }

        public TrackJunction SpawnFromPrefab(TrackSegment segment, Transform knot)
        {
            var instance = Instantiate(this);
            instance.segment = segment;
            instance.transform.position = knot.position;
            instance.transform.rotation = RotationOffset(knot.rotation);
            
            return instance;
        }
        
        private void OnValidate()
        {
            if (segment)
            {
                var t = connectionEnd == ConnectionEnd.Start ? 0.0f : 1.0f;
                transform.position = segment.SamplePoint(t);
                var tangent = segment.SampleTangent(t);
                
                transform.rotation = tangent.magnitude > 0.5f ? Quaternion.LookRotation(tangent, Vector3.up) : Quaternion.identity;
            }
            
            FindConnectionEnd();

            Animate(testActive ? 1.0f : 0.0f);
        }

        private void Awake()
        {
            Visuals = GetComponentsInChildren<Renderer>();
        }

        private void Start()
        {
            if (!segment)
            {
                gameObject.SetActive(false);
                return;
            }
            
            FindConnectionEnd();
            var knot = segment[connectionEnd switch 
            {
                ConnectionEnd.Start => 0,
                ConnectionEnd.End => segment.FromEnd(1),
                _ => throw new ArgumentOutOfRangeException()
            }];

            SetState(GetState());
            
            transform.position = knot.position;
            transform.rotation = RotationOffset(knot.rotation);
        }

        private Quaternion RotationOffset(Quaternion origin) => connectionEnd switch 
        {
            ConnectionEnd.Start => Quaternion.identity,
            ConnectionEnd.End => Quaternion.Euler(0.0f, 180.0f, 0.0f) * origin,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        private void FindConnectionEnd()
        {
            if (!TrackSegment.Valid(segment)) return;

            var knotStart = segment[0].position;
            var knotEnd = segment[segment.FromEnd(1)].position;

            var sd = (knotStart - transform.position).magnitude;
            var ed = (knotEnd - transform.position).magnitude;

            connectionEnd = sd < ed ? ConnectionEnd.Start : ConnectionEnd.End;
        }

        private void FixedUpdate()
        {
            state = GetState();
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

        public void Nudge(int direction)
        {
            SetState(direction == 1);
        }

        public void BeginInteract(GameObject interactingObject)
        {
            SetState(!GetState());
        }
        
        public TrackSegment.Connection GetConnection() => connectionEnd switch
        {
            ConnectionEnd.Start => segment.StartConnection,
            ConnectionEnd.End => segment.EndConnection,
            _ => throw new ArgumentOutOfRangeException()
        };

        public bool GetState() => GetConnection().connectionActive;

        public void SetState(bool state) => GetConnection().connectionActive = state;

        public void ContinueInteract(GameObject interactingObject) { }
    }
}