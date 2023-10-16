using System.Collections.Generic;
using FMODUnity;
using FR8Runtime.CodeUtility;
using FR8Runtime.Interactions.Drivables;
using FR8Runtime.Interactions.Drivers.Submodules;
using FR8Runtime.References;
using FR8Runtime.Rendering.Passes;
using UnityEngine;

namespace FR8Runtime.Interactions.Drivers
{
    [SelectionBase, DisallowMultipleComponent]
    public class Door : MonoBehaviour, IDriver
    {
        [SerializeField] private bool testValue;
        [SerializeField] private TwoPoseDrivableAnimator animator;

        private Renderer[] visuals;
        
        public string Key => string.Empty;
        public virtual bool CanInteract => true;
        public string DisplayName => "Door";
        public virtual string DisplayValue => Open ? "Open" : "Closed";
        public bool Open { get; private set; }

        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new System.NotImplementedException();

        protected virtual void Awake()
        {
            SetValue(testValue ? 1.0f : 0.0f);
            visuals = GetComponentsInChildren<Renderer>();
        }

        public void OnValueChanged(float newValue)
        {
            SoundUtility.PlayOneShot(Open ? SoundReference.DoorOpen : SoundReference.DoorClosed, gameObject);
        }

        protected virtual void SetValue(float newValue)
        {
            Open = newValue > 0.5f;
            animator.SetValue(Open ? 1.0f : 0.0f);
        }

        public void Nudge(int direction)
        {
            SetValue(direction);
        }

        public void BeginInteract(GameObject interactingObject)
        {
            SetValue(Open ? 0.0f : 1.0f);
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