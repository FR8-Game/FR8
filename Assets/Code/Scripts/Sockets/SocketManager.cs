using System;
using System.Text.RegularExpressions;
using FR8Runtime.Interactions.Drivers.Submodules;
using FR8Runtime.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FR8Runtime.Sockets
{
    [System.Serializable]
    public class SocketManager : MonoBehaviour, IInteractable
    {
        [SerializeField] private string regexFilter = @".*";

        private new Collider[] collider;
        private ISocketable currentBinding;

        public bool CanInteract => !(Object)currentBinding;
        public string DisplayName => name;
        public virtual string DisplayValue => string.Empty;
        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new NotImplementedException();
        
        protected virtual void Awake()
        {
            collider = GetComponentsInChildren<Collider>();
            SetCollision(true);
        }

        public void Bind(ISocketable socketable)
        {
            var filterRegex = new Regex(regexFilter, RegexOptions.IgnoreCase);

            if (!socketable.CanBind()) return;
            if (!filterRegex.IsMatch(socketable.SocketType)) return;
            
            currentBinding = socketable.Bind(this);

            SetCollision(false);
        }

        public void Unbind()
        {
            if (!(Object)currentBinding) return;

            currentBinding = currentBinding.Unbind();
            
            SetCollision(true);
        }

        public void SetCollision(bool state)
        {
            foreach (var e in collider) e.enabled = state;
        }
        
        public void Nudge(int direction) { }

        public void BeginInteract(GameObject interactingObject)
        {
            if ((Object)currentBinding) return;
            
            var avatar = interactingObject.GetComponentInParent<PlayerAvatar>();
            if (!avatar) return;
            
            var interactionManager = avatar.interactionManager;
            if (!interactionManager.HeldObject) return;
            if (interactionManager.HeldObject is not ISocketable socketable) return;

            interactionManager.Drop();
            Bind(socketable);
        }
        
        public void ContinueInteract(GameObject interactingObject) { }
    }
}