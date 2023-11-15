using FR8.Runtime.Interactions.Drivables;
using FR8.Runtime.Interactions.Drivers.Submodules;
using FR8.Runtime.References;
using FR8.Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8.Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class Door : MonoBehaviour, IDriver
    {
        public bool isFuzeDoor;

        [SerializeField] private bool testValue;
        [SerializeField] private TwoPoseDrivableAnimator animator;

        private Renderer[] visuals;

        public string Key => string.Empty;
        public virtual bool CanInteract => true;
        public string DisplayName => "Door";
        public virtual string DisplayValue => Open ? "Open" : "Closed";
        public bool Open { get; private set; }
        public IInteractable.InteractionType Type => IInteractable.InteractionType.Press;

        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new System.NotImplementedException();

        protected virtual void Awake()
        {
            SetValue(testValue ? 1.0f : 0.0f);
            visuals = GetComponentsInChildren<Renderer>();
        }

        public void OnValueChanged(float newValue)
        {
            //Debug.Log("Door Sound?");
            //SoundReference.DoorOpen.PlayOneShot();
            //SoundUtility.PlayOneShot(Open ? SoundReference.DoorOpen : SoundReference.DoorClosed, gameObject);
        }

        protected virtual void SetValue(float newValue)
        {
            Open = newValue > 0.5f;
            animator.SetValue(Open ? 1.0f : 0.0f);
        }

        public void Nudge(int direction)
        {
            SetValue(direction);
            if (isFuzeDoor) SoundReference.FuzeDoor.PlayOneShot();
            else SoundReference.DoorOpen.PlayOneShot();

        }

        public void BeginInteract(GameObject interactingObject)
        {
            SetValue(Open ? 0.0f : 1.0f);
            if (isFuzeDoor) SoundReference.FuzeDoor.PlayOneShot();
            else SoundReference.DoorOpen.PlayOneShot();
        }

        public void ContinueInteract(GameObject interactingObject)
        {
        }

        public void Highlight(bool highlight)
        {
            if (highlight) SelectionOutlinePass.Add(visuals);
            else SelectionOutlinePass.Remove(visuals);
        }

        private void Update()
        {
            animator.Update();
        }

        protected virtual void FixedUpdate()
        {
            animator.FixedUpdate();
        }

        private void OnValidate()
        {
            animator.OnValidate(testValue ? 1.0f : 0.0f);
        }
    }
}