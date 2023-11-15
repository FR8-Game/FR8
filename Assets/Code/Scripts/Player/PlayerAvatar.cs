using System;
using System.Collections.Generic;
using FR8.Runtime.Player.Submodules;
using UnityEngine;

namespace FR8.Runtime.Player
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
        //public PlayerInventory inventory;
        public PlayerUrination urination;
        
        public Func<Vector3> getCenter;

        public event Action EnableEvent;
        public event Action StartEvent;
        public event Action UpdateEvent;
        public event Action FixedUpdateEvent;
        public event Action DisableEvent;
        public event Action<PlayerMount> MountChangedEvent;

        public bool IsAlive => vitality.IsAlive;
        public Transform Head { get; private set; }
        public Rigidbody Body { get; private set; }
        public PlayerMount CurrentMount { get; private set; }

        public Vector3 MoveDirection
        {
            get
            {
                var input = this.input.Move;
                return transform.TransformDirection(input.x, 0.0f, input.z);
            }
        }

        public static readonly List<PlayerAvatar> All = new();

        private void Awake()
        {
            transform.SetParent(null);

            Head = transform.Find("Head");
            Head.SetParent(transform);
            Head.localPosition = Vector3.zero;
            Head.localRotation = Quaternion.identity;

            Body = gameObject.GetOrAddComponent<Rigidbody>();
            Body.isKinematic = true;

            getCenter = () => transform.position;
            
            InitSubmodules();
        }

        private void OnEnable()
        {
            All.Add(this);
            EnableEvent?.Invoke();
        }

        private void OnDisable()
        {
            All.Remove(this);
            DisableEvent?.Invoke();
        }

        private void Start()
        {
            StartEvent?.Invoke();
        }

        private void InitSubmodules()
        {
            input.Init(this);
            cameraController.Init(this);
            interactionManager.Init(this);
            //inventory.Init(this);
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

        public void SetMount(PlayerMount mount)
        {
            MountChangedEvent?.Invoke(mount);
            if (mount) Toast.ShowToast($"Press {input.crouchAction.action.bindings[0].name} to leave {mount.DisplayName}", Color.white);
            CurrentMount = mount;
        }
    }
}