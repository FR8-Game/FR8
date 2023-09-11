using System;
using System.IO;
using System.Text;
using FR8Runtime.Contracts;
using UnityEngine;

namespace FR8Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerContractManager : MonoBehaviour
    {
        [SerializeField] private TextAsset initialContract;

        public Contract ActiveContract { get; private set; }

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
            if (ActiveContract) DestroyImmediate(ActiveContract);
            if (!initialContract) return;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(initialContract.text)))
            {
                stream.Position = 0;
                ActiveContract = Contract.Deserialize(stream);
            }
        }
    }
}