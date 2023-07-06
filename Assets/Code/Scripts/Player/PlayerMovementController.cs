using System;
using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerMovementController : MonoBehaviour
    {
        private PlayerMovementModule[] modules;

        private void Awake()
        {
            modules = GetComponents<PlayerMovementModule>();
        }

        private void Start()
        {
            SwitchModule<PlayerGroundedMovement>();
        }

        public void SwitchModule<T>() where T : PlayerMovementModule => SwitchModule(typeof(T));

        public void SwitchModule(Type type)
        {
            foreach (var module in modules)
            {
                module.enabled = module.GetType() == type;
            }
        }
    }
}