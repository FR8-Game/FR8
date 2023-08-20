using FR8.Interactions.Drivables;
using FR8.Interactions.Drivers.DragBehaviours;
using FR8.Interactions.Drivers.Submodules;
using FR8.Player;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class GearStick : Driver
    {
        [SerializeField] private int testGear;
        
        [Space]
        [SerializeField] private TwoPoseDrivableAnimator animator;
        [SerializeField] private DriverSounds sounds;
        [SerializeField] private DriverSliderDragBehaviour dragBehaviour;
        
        public override string DisplayValue =>
            Value switch
            {
                > 0.5f => "Drive",
                < -0.5f => "Reverse",
                _ => "Neutral"
            };
        
        public override void OnValueChanged(float newValue)
        {
            animator.SetValue(newValue);
            sounds.SetValue(newValue, 3);
            
            base.OnValueChanged(newValue);
        }
        
        protected override void SetValue(float newValue)
        {
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
            var ray = avatar.cameraController.LookingRay;
            
            dragBehaviour.BeginDrag(transform, Value, ray);
        }

        public override void ContinueInteract(GameObject interactingObject)
        {
            var avatar = interactingObject.GetComponentInParent<PlayerAvatar>();
            if (!avatar) return;
            var ray = avatar.cameraController.LookingRay;
            
            SetValue(dragBehaviour.ContinueDrag(transform, ray));
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            sounds.Awake(gameObject);
        }

        private void Update()
        {
            animator.Update();
        }

        private void FixedUpdate()
        {
            animator.FixedUpdate();
        }

        private void OnValidate()
        {
            testGear = Mathf.Clamp(testGear, -1, 1);
            animator.OnValidate(testGear);
        }
    }
}