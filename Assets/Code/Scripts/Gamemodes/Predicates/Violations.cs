using FR8.Runtime.Player;
using FR8.Runtime.Train.Electrics;
using UnityEngine;

namespace FR8.Runtime.Gamemodes.Predicates
{
    [System.Serializable]
    public class Violations
    {
        [SerializeField] private bool overtime;
        [SerializeField] private int fusesShorted;
        [SerializeField] private int overloadedTrain;
        
        public const int OvertimeCost = 2000;
        public const int FuseShortCost = 500;
        public const int OverloadTrainCost = 500;

        public void OnEnable()
        {
            TrainElectricsController.FuseBlown += OnFuseBlown;
        }
        
        public void OnDisable()
        {
            TrainElectricsController.FuseBlown -= OnFuseBlown;
        }
        
        private void OnFuseBlown(TrainElectricsController train)
        {
            fusesShorted++;
            ShowToast("Main Fuse Blown", FuseShortCost);
        }

        private void ShowToast(string reason, int amount)
        {
            Toast.ShowToast($"{reason} | -${amount}", Color.red);
        }

        public void Overtime()
        {
            if (overtime) return;
            overtime = true;
            ShowToast("Overtime", OvertimeCost);
        }

        public int ComputeReward(int startingCost)
        {
            if (overtime) startingCost -= OvertimeCost;
            startingCost -= fusesShorted * FuseShortCost;
            startingCost -= overloadedTrain * OverloadTrainCost;
            
            return startingCost;
        }
    }
}