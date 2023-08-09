using FR8.Interactions.Drivables;
using FR8.Interactions.Drivers.DragBehaviours;
using FR8.Interactions.Drivers.Submodules;
using FR8.Player;
using UnityEngine;

namespace FR8.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class ControlStick : Driver
    {
        [SerializeField] private int steps = 10;
        [SerializeField] private bool forceStep = true;
        [SerializeField] protected float testValue;
        [SerializeField] protected Vector2 range = Vector2.up;

        [Space]
        [SerializeField] private TwoPoseDrivableAnimator animator;

        [SerializeField] private DriverSounds sounds;
        [SerializeField] private DriverSliderDragBehaviour dragBehaviour;

        public override void OnValueChanged(float newValue)
        {
            animator.SetValue(newValue);
            sounds.SetValue(newValue, steps);

            base.OnValueChanged(newValue);
        }

        protected override void SetValue(float newValue)
        {
            newValue = Mathf.Clamp(newValue, range.x, range.y);
            if (forceStep) newValue = Mathf.Round(newValue * steps) / steps;

            base.SetValue(newValue);
        }

        public override void Nudge(int direction)
        {
            SetValue(Value + (float)direction / steps);
        }

        public override void BeginInteract(GameObject interactingObject)
        {
            var avatar = interactingObject.GetComponentInParent<PlayerGroundedAvatar>();
            if (!avatar) return;
            var ray = avatar.LookingRay;

            dragBehaviour.BeginDrag(transform, Value, ray);
        }

        public override void ContinueInteract(GameObject interactingObject)
        {
            var avatar = interactingObject.GetComponentInParent<PlayerGroundedAvatar>();
            if (!avatar) return;
            var ray = avatar.LookingRay;

            SetValue(dragBehaviour.ContinueDrag(transform, ray));
        }

        protected override void Awake()
        {
            base.Awake();

            sounds.Awake(gameObject);
        }

        protected virtual void Update()
        {
            animator.Update();
        }

        protected virtual void FixedUpdate()
        {
            animator.FixedUpdate();
        }

        protected virtual void OnValidate()
        {
            animator.OnValidate(testValue);
        }
    }
}