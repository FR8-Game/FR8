using System;
using System.Collections.Generic;
using UnityEngine;

namespace FR8.Train.Electrics
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TrainElectricsController : MonoBehaviour
    {
        [SerializeField] private float capacityMegaWattHours;
        [SerializeField] private float baselineGeneration = 150.0f;

        private List<TrainElectrics> electrics;
        
        public float PowerStorage { get; private set; }
        public float PowerDraw { get; private set; }
        public float PowerCapacity => capacityMegaWattHours;
        
        private void Awake()
        {
            electrics = new List<TrainElectrics>(GetComponentsInChildren<TrainElectrics>());

            foreach (var e in electrics) e.SetController(this).Connected = true;
        }

        private void FixedUpdate()
        {
            var deltaPower = baselineGeneration * Time.deltaTime;

            foreach (var e in electrics)
            {
                deltaPower += e.CalculatePowerConsumptionMegawatts() * Time.deltaTime;
            }

            PowerStorage += Mathf.Min(deltaPower, PowerCapacity);
            if (PowerStorage < 0.0f)
            {
                DisconnectFuzeGroup(LastFuzeGroup());
            }

            PowerDraw = deltaPower / Time.deltaTime;
        }

        private int LastFuzeGroup()
        {
            var lastFuzeGroup = 0;
            foreach (var e in electrics)
            {
                lastFuzeGroup = Mathf.Max(lastFuzeGroup, e.FuzeGroup);
            }

            return lastFuzeGroup;
        }

        private void UpdateFuzeGroup(int fuzeGroup, Action<TrainElectrics> callback)
        {
            foreach (var e in electrics)
            {
                if (e.FuzeGroup != fuzeGroup) continue;
                callback(e);
            }
        }

        public void ConnectFuzeGroup(int fuzeGroup) => UpdateFuzeGroup(fuzeGroup, e => e.Connected = true);
        public void DisconnectFuzeGroup(int fuzeGroup) => UpdateFuzeGroup(fuzeGroup, e => e.Connected = false);
    }
}
