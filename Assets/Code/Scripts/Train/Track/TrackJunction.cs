using System;
using FR8.Runtime.Editor;
using FR8.Runtime.Interactions.Drivers.Submodules;
using FR8.Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8.Runtime.Train.Track
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrackJunction : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool stateOverride;
        [SerializeField] private MeshRenderer target;
        [SerializeField] private int materialIndex = 2;
        [SerializeField] private string materialProperty = "_Light_Intensity";
        [Range(0.0f, 1.0f)]
        [SerializeField] private float valueSmoothing = 0.3f;

        [Space]
        [SerializeField] private ConnectionEnd connectionEnd;
        [SerializeField] [ReadOnly] private TrackSegment main;
        [SerializeField] [ReadOnly] private TrackSegment other;

        private bool state;
        private Renderer[] visuals;
        private float positionOnSpline;
        private MaterialPropertyBlock propertyBlock;
        private float smoothedValue;

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
                SetState(stateOverride);
            }
        }

        private void Awake() { visuals = GetComponentsInChildren<Renderer>(); }

        private void Start()
        {
            if (!main || !other)
            {
                enabled = false;
                return;
            }

            propertyBlock = new MaterialPropertyBlock();

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
            smoothedValue = Mathf.Lerp(state ? 1.0f : 0.0f, smoothedValue, valueSmoothing);

            propertyBlock.SetFloat(materialProperty, smoothedValue);
            if (target)
            {
                target.SetPropertyBlock(propertyBlock);
            }

            if (state)
            {
                foreach (var t in TrainCarriage.All)
                {
                    if (t.Segment != other) continue;
                    ProcessTrain(t);
                }
            }
        }

        private void ProcessTrain(TrainCarriage train)
        {
            var position = connectionEnd switch
            {
                ConnectionEnd.Start => main[0].position,
                ConnectionEnd.End => main[^1].position,
                _ => throw new ArgumentOutOfRangeException()
            };
            var normal = transform.forward;

            var next = train.Body.position;
            var last = next - train.Body.velocity * Time.deltaTime;

            var dotLast = Vector3.Dot(normal, last - position);
            var dotCurrent = Vector3.Dot(normal, next - position);

            if (dotCurrent >= 0.0f && dotLast < 0.0f)
            {
                train.Segment = main;
            }
        }

        public enum ConnectionEnd
        {
            Start,
            End
        }

        public void Nudge(int direction) { SetState(direction == 1); }

        public void BeginInteract(GameObject interactingObject) { SetState(!GetState()); }

        public TrackSegment.Connection GetConnection()
        {
            if (!main) return null;
            return connectionEnd switch
            {
                ConnectionEnd.Start => main.StartConnection,
                ConnectionEnd.End => main.EndConnection,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool GetState() => GetConnection()?.active ?? false;

        public void SetState(bool state)
        {
            var c = GetConnection();
            if (c == null) return;
            c.active = state;
        }

        public void ContinueInteract(GameObject interactingObject) { }

        public void Highlight(bool highlight)
        {
            if (highlight) SelectionOutlinePass.Add(visuals);
            else SelectionOutlinePass.Remove(visuals);
        }

        private void Reset() { target = transform.Find<MeshRenderer>("SignalPostJunction/JunctionSignalLight/JunctionSignalLight_GEO"); }
    }
}