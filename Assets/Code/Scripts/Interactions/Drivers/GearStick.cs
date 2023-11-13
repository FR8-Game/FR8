using FR8.Runtime.Interactions.Drivables;
using FR8.Runtime.Interactions.Drivers.DragBehaviours;
using FR8.Runtime.Interactions.Drivers.Submodules;
using FR8.Runtime.Player;
using FR8.Runtime.Train.Electrics;
using UnityEngine;

namespace FR8.Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class GearStick : Driver
    {
        [SerializeField] private int testGear;
        
        [Space]
        [SerializeField] private TwoPoseDrivableAnimator animator;
        [SerializeField] private DriverSounds sounds;
        [SerializeField] private DriverSliderDragBehaviour dragBehaviour;

        private TrainGasTurbine engine;
        
        public override string DisplayValue
        {
            get
            {
                if (!engine.running) return "Neutral [Locked: Engine is not Running]";
                
                return Value switch
                {
                    > 0.5f => "Drive",
                    < -0.5f => "Reverse",
                    _ => "Neutral"
                };
            }
        }

        public override void OnValueChanged(float newValue)
        {
            animator.SetValue(newValue);
            sounds.SetValue(newValue, 3);
            
            base.OnValueChanged(newValue);
        }
        
        protected override void SetValue(float newValue)
        {
            if (!engine.running)
            {
                base.SetValue(0.0f);
                Shake();
                return;
            }
            
            switch (newValue)
            {
                case > 0.5f:
                    base.SetValue(1.0f);
                    break;
                case < -0.5f:
                    base.SetValue(-1.0f);
                    break;
                default:
                    base.SetValue(0.0f);
                    break;
            }
        }

        public override void Nudge(int direction)
        {
            SetValue(Value + direction);
        }

        public override void BeginInteract(GameObject interactingObject)
        {
            var avatar = interactingObject.GetComponentInParent<PlayerAvatar>();
            if (!avatar) return;
            var ray = avatar.cameraController.LookRay;
            
            dragBehaviour.BeginDrag(transform, Value, ray);
        }

        public override void ContinueInteract(GameObject interactingObject)
        {
            var avatar = interactingObject.GetComponentInParent<PlayerAvatar>();
            if (!avatar) return;
            var ray = avatar.cameraController.LookRay;
            
            SetValue(dragBehaviour.ContinueDrag(transform, ray));
        }
        
        protected override void Awake()
        {
            base.Awake();
            engine = GetComponentInParent<TrainGasTurbine>();
            
            sounds.Awake(gameObject);
        }

        private void Update()
        {
            animator.Update();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            animator.FixedUpdate();
        }

        private void OnValidate()
        {
            testGear = Mathf.Clamp(testGear, -1, 1);
            animator.OnValidate(testGear);
        }
    }
}