using System;
using UnityEngine;

namespace FR8.Player
{
    [Serializable]
    public abstract class PlayerAvatar : MonoBehaviour
    {
        public PlayerController Controller { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        protected virtual void Awake()
        {
            if (!Controller) Release();
            
            Configure();
        }

        protected virtual void Configure()
        {
            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
        }

        public void Possess(PlayerController controller)
        {
            enabled = true;
            Controller = controller;
        }

        public void Release()
        {
            enabled = false;
        }
    }
}