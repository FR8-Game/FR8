using FR8.Dialogue;
using FR8.Interactions.Drivers;
using FR8.Player;
using FR8.Train.Electrics;
using UnityEngine;

namespace FR8.Train
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrainMonitor : MonoBehaviour
    {
        public const float MaxTrainSafeSpeed = 10.0f / 3.6f;
        
        [SerializeField] private DialogueSource source;
        [SerializeField] private Bounds cockpitBounds;

        private TrainCarriage carriage;
        private TrainElectricsController trainElectrics;
        private DriverNetwork trainDrivers;

        private void Awake()
        {
            carriage = GetComponent<TrainCarriage>();
            trainDrivers = GetComponent<DriverNetwork>();
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
            
            trainElectrics.SetMainFuse(false);
            trainDrivers.SetValue("Brake", 1.0f);
            
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