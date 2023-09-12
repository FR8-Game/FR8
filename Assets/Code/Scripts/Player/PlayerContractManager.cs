using System;
using System.Collections.Generic;
using FR8Runtime.Contracts;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerContractManager : MonoBehaviour
    {
        [SerializeField] private List<Contract> initialContracts;

        public List<Contract> ActiveContracts { get; private set; }

        private void OnEnable()
        {
            ActiveContracts = new List<Contract>(initialContracts);
        }

        private void Update()
        {
            foreach (var e in ActiveContracts)
            {
                e.Update();
            }
        }
    }
}