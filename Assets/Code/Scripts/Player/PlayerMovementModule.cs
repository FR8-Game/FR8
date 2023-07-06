using System;
using UnityEngine;

namespace FR8.Player
{
    [Serializable]
    public abstract class PlayerMovementModule : MonoBehaviour
    {
        public PlayerController Controller { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        protected virtual void Awake()
        {
            Controller = GetComponent<PlayerController>();
            Rigidbody = GetComponent<Rigidbody>();
        }
    }
}