using System;
using UnityEngine;

namespace FR8Runtime.Contracts.Predicates
{
    [Serializable]
    public class DeliveryPredicate : ContractPredicate
    {
        [SerializeField] private string[] carriages;
        [SerializeField] private string deliveryLocation;

        protected override int TasksDone()
        {
            var count = 0;
            foreach (var e in carriages)
            {
                if (deliveryLocation.Contains(e)) count++;
            }

            return count;
        }

        protected override int TaskCount() => carriages.Length;
    }
}