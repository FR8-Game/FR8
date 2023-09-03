using System.Collections.Generic;
using FR8Runtime.Dialogue;
using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Player;
using FR8Runtime.Shapes;
using FR8Runtime.Train.Electrics;
using UnityEngine;

namespace FR8Runtime.Train
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrainMonitor : MonoBehaviour
    {
        public const float MaxTrainSafeSpeed = 10.0f / 3.6f;
        
        [SerializeField] private DialogueSource source;

        private TrainCarriage carriage;
        private TrainElectricsController trainElectrics;
        private DriverNetwork driverNetwork;

        private List<Shape> shapes = new();

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
                foreach (var s in shapes)
                {
                    if (s.ContainsPoint(p.getCenter())) return;
                }
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
            GetShapes();

            foreach (var e in shapes)
            {
                e.DrawGizmos();
            }
        }

        private void GetShapes()
        {
            Shape.FindShapes(shapes, transform, "Shapes/Cockpit", true, true);
        }
    }
}