using System;
using System.Text;
using FR8Runtime.Train;
using UnityEngine;

namespace FR8Runtime.Contracts.Predicates
{
    [Serializable]
    [ContractElement]
    public class DeliveryPredicate : ContractPredicate
    {
        public string[] carriageNames;
        public string deliveryLocationName;

        public override string BuildText()
        {
            var sb = new StringBuilder();

            sb.Append("Deliver ");
            for (var i = 0; i < carriageNames.Length; i++)
            {
                if (i == 0) sb.Append($"{carriageNames[i]}");
                else if (i == sb.Length - 1) sb.Append($", {carriageNames[i]}");
                else sb.Append($", and {carriageNames[i]}");
            }
            sb.Append($" to {deliveryLocationName}");
            
            return sb.ToString();
        }

        protected override int TasksDone()
        {
            var carriages = new TrainCarriage[carriageNames.Length];
            var deliveryLocation = GameObject.Find(deliveryLocationName).GetComponent<TrackSection>();

            for (var i = 0; i < carriages.Length; i++)
            {
                carriages[i] = GameObject.Find(carriageNames[i]).GetComponent<TrainCarriage>();
            }
            
            var count = 0;
            foreach (var e in carriages)
            {
                if (deliveryLocation.Contains(e.transform.position)) count++;
            }

            return count;
        }

        protected override int TaskCount() => carriageNames.Length;
    }
}