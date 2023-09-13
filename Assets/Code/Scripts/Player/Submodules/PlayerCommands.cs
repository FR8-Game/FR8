
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace FR8Runtime.Player.Submodules
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerAvatar))]
    public class PlayerCommands : MonoBehaviour
    {
        private PlayerAvatar avatar;

        private Command[] commands;

        private void Awake()
        {
            avatar = GetComponent<PlayerAvatar>();
            
            commands = new[]
            {
                new Command(Keyboard.current.dKey, () =>
                {
                    avatar.vitality.SetShields(0.0f);
                    avatar.vitality.SetHealth(1);
                })
            };
        }

        private void Update()
        {
            if (!Application.isEditor) return;
            if (!(Keyboard.current.leftAltKey.isPressed && Keyboard.current.leftShiftKey.isPressed)) return;

            foreach (var c in commands) c.Try();
        }

        public class Command
        {
            private KeyControl key;
            private Action callback;

            public Command(KeyControl key, Action callback)
            {
                this.key = key;
                this.callback = callback;
            }

            public void Try()
            {
                if (key.wasPressedThisFrame) callback();
            }
        }
    }
}