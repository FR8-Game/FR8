using System;
using FR8Runtime.Interactions.Drivers.Submodules;
using UnityEngine;

namespace FR8Runtime.Level.Fuel
{
    [SelectionBase, DisallowMultipleComponent]
    public class FuelPump : MonoBehaviour, IInteractable
    {
        [SerializeField] private float maxRange;
        [SerializeField] private float bounce = 0.6f;
        [SerializeField] private Transform model;
        [SerializeField] private Transform handleAnchor;
        [SerializeField] private float rotationStep = 11.25f;
        [SerializeField] private float refuelRate = 2.0f;

        [SerializeField] private CodeUtility.DampedSpring rotationSpring;

        private CodeUtility.Rope rope;
        private LineRenderer lines;
        private FuelPumpHandle handle;

        private void Awake()
        {
            lines = GetComponentInChildren<LineRenderer>();
            handle = GetComponentInChildren<FuelPumpHandle>();
            handle.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            rope = new CodeUtility.Rope();
        }

        private void Start()
        {
            handle.transform.position = handleAnchor.position + Vector3.down * maxRange;
        }

        private void Update()
        {
            const int resolution = 32;
            lines.useWorldSpace = true;
            lines.positionCount = resolution;

            for (var i = 0; i < resolution; i++)
            {
                var p = i / (resolution - 1.0f);
                lines.SetPosition(i, rope.Sample(p));
            }
        }

        private void FixedUpdate()
        {
            rotationSpring.Iterate(Time.deltaTime);
            model.localRotation = Quaternion.Euler(0.0f, rotationSpring.currentPosition * rotationStep, 0.0f);

            var vector = (handle.Rigidbody.position - handleAnchor.position);
            var distance = vector.magnitude;
            var direction = vector / distance;

            if (distance > maxRange)
            {
                if (handle.CurrentBinding) handle.CurrentBinding.Unbind();

                var displacement = direction * maxRange;
                handle.Rigidbody.position = handleAnchor.position + displacement;

                var newVelocity = handle.Rigidbody.velocity - direction * Mathf.Max(Vector3.Dot(direction, handle.Rigidbody.velocity), 0.0f) * (1.0f + bounce);
                handle.Rigidbody.AddForce(newVelocity - handle.Rigidbody.velocity, ForceMode.VelocityChange);
            }

            handle.Rigidbody.rotation = Quaternion.identity;

            rope.Update(handleAnchor.position, handle.Rigidbody.position, maxRange);

            if (handle.Engine)
            {
                handle.Engine.Refuel(refuelRate);
            }
        }

        private void OnDrawGizmos()
        {
            if (rope != null)
            {
                Gizmos.color = Color.red;
                rope.DrawGizmos();
            }

            var lines = GetComponentInChildren<LineRenderer>();
            if (lines) return;
            if (!handleAnchor) return;
            Gizmos.color = Color.green;

            Gizmos.DrawRay(handleAnchor.position, Vector3.down * maxRange);
        }

        private void OnValidate()
        {
            if (!handleAnchor) return;

            var lines = GetComponentInChildren<LineRenderer>();
            if (lines)
            {
                lines.useWorldSpace = true;
                lines.SetPosition(0, handleAnchor.position);
                lines.SetPosition(1, handleAnchor.position + Vector3.down * maxRange);
            }

            var handle = GetComponentInChildren<FuelPumpHandle>();
            if (handle)
            {
                handle.transform.position = handleAnchor.transform.position + Vector3.down * maxRange;
                handle.transform.rotation = UnityEngine.Quaternion.identity;
            }
        }

        public bool CanInteract => true;
        public string DisplayName => "Fuel Gantry";
        public string DisplayValue => "Rotate";
        public bool OverrideInteractDistance => false;
        public float InteractDistance => throw new NotImplementedException();

        public void Nudge(int direction)
        {
            rotationSpring.Target(rotationSpring.targetPosition + direction);
        }

        public void BeginInteract(GameObject interactingObject) { }

        public void ContinueInteract(GameObject interactingObject) { }
    }
}