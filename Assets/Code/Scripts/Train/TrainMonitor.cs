﻿using FR8Runtime.Dialogue;
using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Player;
using FR8Runtime.Train.Electrics;
using UnityEngine;

namespace FR8Runtime.Train
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrainMonitor : MonoBehaviour
    {
        public const float MaxTrainSafeSpeed = 10.0f / 3.6f;
        
        [SerializeField] private DialogueSource source;
        [SerializeField] private Bounds cockpitBounds;

        private TrainCarriage carriage;
        private TrainElectricsController trainElectrics;
        private DriverNetwork driverNetwork;

        private void Awake()
        {
            carriage = GetComponent<TrainCarriage>();
            driverNetwork = GetComponent<DriverNetwork>();
            trainElectrics = GetComponent<TrainElectricsController>();
        }

        private void FixedUpdate()
        {
            CheckForDriver();
        }

        private void CheckForDriver()
        {
            var fwdSpeed = carriage.GetForwardSpeed();
            if (Mathf.Abs(fwdSpeed) < MaxTrainSafeSpeed) return;
            
            var players = FindObjectsOfType<PlayerAvatar>();
            foreach (var p in players)
            {
                var point = transform.InverseTransformPoint(p.transform.position);
                if (cockpitBounds.Contains(point)) return;
            }

            if (!trainElectrics.GetConnected()) return;
            
            trainElectrics.SetConnected(false);
            driverNetwork.SetValue("Brake", 1.0f);
            
            var entry = new DialogueEntry()
            {
                source = source,
                body = "Train does not contain a driver.\nStopping Train."
            };
            
            DialogueListener.QueueDialogue(entry);
        }

        private void OnEnable()
        {
            trainElectrics.FuseBlown += OnFuseBlown;
        }

        private void OnDisable()
        {
            trainElectrics.FuseBlown -= OnFuseBlown;
        }

        private void OnFuseBlown()
        {
            var entry = new DialogueEntry()
            {
                source = source,
                body = "Your Train has Tripped the Main Fuse.\nThe Main Fuse can be reset from the Fuze Box in the Trains Cockpit."
            };
            
            DialogueListener.QueueDialogue(entry);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Gizmos.DrawWireCube(cockpitBounds.center, cockpitBounds.size);
        }
    }
}