using System;
using FR8Runtime.Player.Submodules;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerAvatar : MonoBehaviour
    {
        public PlayerInput input;
        public PlayerInteractionManager interactionManager;
        public PlayerCamera cameraController;
        public PlayerVitality vitality;
        public PlayerUI ui;
        public PlayerInventory inventory;
        public PlayerUrination urination;
        
        public Func<Vector3> getCenter;

        public event Action UpdateEvent;
        public event Action FixedUpdateEvent;

        public bool IsAlive => vitality.IsAlive;
        public Transform Head { get; private set; }
        public Rigidbody Body { get; private set; }

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
            transform.SetParent(null);
            InitSubmodules();

            Head = new GameObject("Head").transform;
            Head.SetParent(transform);
            Head.localPosition = Vector3.zero;
            Head.localRotation = Quaternion.identity;

            Body = gameObject.GetOrAddComponent<Rigidbody>();
            Body.isKinematic = true;

            getCenter = () => transform.position;
        }

        private void InitSubmodules()
        {
            input.Init(this);
            cameraController.Init(this);
            interactionManager.Init(this);
            inventory.Init(this);
            vitality.Init(this);
            ui.Init(this);
            urination.Init(this);
        }
        private void Update()
        {
            UpdateEvent?.Invoke();
        }

        private void FixedUpdate()
        {
            FixedUpdateEvent?.Invoke();
        }

        public void OnValidate()
        {
            ui.OnValidate(this);
        }
    }
}