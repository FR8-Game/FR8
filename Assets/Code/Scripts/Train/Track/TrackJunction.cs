using System;
using FR8Runtime.Interactions.Drivers.Submodules;
using FR8Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8Runtime.Train.Track
{
    public class TrackJunction : MonoBehaviour, IInteractable
    {
        [SerializeField] private TrackSegment main;
        [SerializeField] private TrackSegment other;
        [SerializeField] private Transform primaryIndicator;
        [SerializeField] private Transform secondaryIndicator;
        [SerializeField] private CodeUtility.DampedSpring animationSpring;
        [SerializeField] private bool flip;
        [SerializeField] private bool testActive;

        [Space]
        [SerializeField] private ConnectionEnd connectionEnd;

        private bool state;
        private Renderer[] visuals;

        public bool CanInteract => true;
        public string DisplayName => "Train Signal";
        public string DisplayValue => state ? "Engaged" : "Disengaged";
        public bool OverrideInteractDistance => true;
        public float InteractDistance => float.MaxValue;
        public IInteractable.InteractionType Type => IInteractable.InteractionType.Press;

        public TrackJunction SpawnFromPrefab(TrackSegment main, TrackSegment other, ConnectionEnd end)
        {
            var t = end == ConnectionEnd.Start ? 0.0f : 1.0f;
            var s = end == ConnectionEnd.Start ? 1.0f : -1.0f;
            
            var instance = Instantiate(this);
            instance.main = main;
            instance.other = other;
            instance.transform.position = main.SamplePoint(t);
            instance.transform.rotation = Quaternion.LookRotation(main.SampleTangent(t) * s, Vector3.up);

            return instance;
        }

        private void OnValidate()
        {
            FindConnectionEnd();

            if (Application.isPlaying)
            {
                SetState(testActive);
            }
            else
            {
                Animate(testActive ? 1.0f : 0.0f);
            }
        }

        private void Awake()
        {
            visuals = GetComponentsInChildren<Renderer>();
        }

        private void Start()
        {
            if (!main)
            {
                gameObject.SetActive(false);
                return;
            }

            FindConnectionEnd();
            SetState(GetState());
        }

        private void FindConnectionEnd()
        {
            if (!TrackSegment.Valid(main)) return;

            var knotStart = main[0].position;
            var knotEnd = main[main.FromEnd(1)].position;

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

        public enum ConnectionEnd
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
            ConnectionEnd.Start => main.StartConnection,
            ConnectionEnd.End => main.EndConnection,
            _ => throw new ArgumentOutOfRangeException()
        };

        public bool GetState() => GetConnection().active;
        public void SetState(bool state) => GetConnection().active = state;

        public void ContinueInteract(GameObject interactingObject) { }

        public void Highlight(bool highlight)
        {
            if (highlight) SelectionOutlinePass.Add(visuals);
            else SelectionOutlinePass.Remove(visuals);
        }

        private void OnTriggerEnter(Collider c)
        {
            if (!GetState()) return;
            var train = c.GetComponentInParent<TrainCarriage>();
            if (!train) return;

            if (train.Segment == main) train.Segment = other;
            else if (train.Segment == other) train.Segment = main;

        }
    }
}