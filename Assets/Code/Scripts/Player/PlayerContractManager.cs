using System;
using System.IO;
using FR8Runtime.Contracts;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerContractManager : MonoBehaviour
    {
        [SerializeField] private TextAsset initialContract;

        private Contract contract;

        private void OnEnable()
        {
            ReadContract();
        }

        private void OnValidate()
        {
            ReadContract();
        }

        private void ReadContract()
        {
            if (contract) DestroyImmediate(contract);

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(initialContract.text);
                contract = Contract.Deserialize(stream);
            }
        }
    }
}