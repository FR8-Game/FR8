using UnityEngine;
using UnityEngine.InputSystem;

namespace FR8.Player.Submodules
{
    public class InputAccelerator
    {
        public float baseValue;
        public float delay;
        public float acceleration;

        private float pressTime;
        private float threshold = 0.5f;

        public InputActionReference reference;

        public float RawValue => reference.action?.ReadValue<float>() ?? 0.0f;
        public bool State => RawValue > threshold;
        public float Value
        {
            get
            {
                var a = Mathf.Max(pressTime - delay, 0.0f);
                var b = 0.5f * a * a;
                var c = baseValue + b * acceleration;
                return State ? c * Mathf.Sign(RawValue) : 0.0f;
            }
        }

        public InputAccelerator(InputActionReference reference)
        {
            this.reference = reference;
        }

        public void Update()
        {
            if (State)
            {
                pressTime += Time.deltaTime;
                return;
            }

            pressTime = 0.0f;
        }
    }
}