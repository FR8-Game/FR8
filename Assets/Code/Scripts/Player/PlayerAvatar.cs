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
            Controller = transform.parent.gameObject.GetOrAddComponent<PlayerController>();
            Rigidbody = Controller.gameObject.GetOrAddComponent<Rigidbody>();
        }
    }
}