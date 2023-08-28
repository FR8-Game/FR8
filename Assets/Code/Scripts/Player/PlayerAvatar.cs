using System;
using FR8Runtime.Player.Submodules;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class PlayerAvatar : MonoBehaviour
    {
        public PlayerInput input;
        public PlayerGroundedMovement groundedMovement;
        public PlayerInteractionManager interactionManager;
        public PlayerCamera cameraController;
        public PlayerVitality vitality;
        public PlayerVitalityFX vitalityFX;
        public PlayerInventory inventory;
        public PlayerUrination urination;

        public event Action EnabledEvent;
        public event Action UpdateEvent;
        public event Action FixedUpdateEvent;
        public event Action DisabledEvent;

        public Rigidbody Rigidbody { get; private set; }
        public Vector3 Center => transform.position + Vector3.up * groundedMovement.CollisionHeight / 2.0f;
        public bool IsAlive => vitality.IsAlive;

        public Vector3 MoveDirection
        {
            get
            {
                var input = this.input.Move;
                return transform.TransformDirection(input.x, 0.0f, input.z);
            }
        }

        private void Awake()
        {
            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            
            transform.SetParent(null);
            InitSubmodules();
        }

        private void InitSubmodules()
        {
            input.Init(this);
            cameraController.Init(this);
            interactionManager.Init(this);
            groundedMovement.Init(this);
            inventory.Init(this);
            vitality.Init(this);
            vitalityFX.Init(this);
            urination.Init(this);
        }

        private void OnEnable()
        {
            EnabledEvent?.Invoke();
        }

        private void OnDisable()
        {
            DisabledEvent?.Invoke();
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
        }

        private void FixedUpdate()
        {
            SyncWithCameraTarget();

            FixedUpdateEvent?.Invoke();
        }

        private void SyncWithCameraTarget()
        {
            Rigidbody.rotation = Quaternion.Euler(0.0f, cameraController.CameraTarget.eulerAngles.y, 0.0f);
            cameraController.CameraTarget.localRotation = Quaternion.identity;
            cameraController.CameraTarget.localPosition = cameraController.cameraOffset;
        }

        private void OnDrawGizmos()
        {
            groundedMovement.DrawGizmos(this);
        }

        public void OnValidate()
        {
            vitalityFX.OnValidate(this);
        }
    }
}