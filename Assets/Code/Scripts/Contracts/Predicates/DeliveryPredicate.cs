using System.Collections.Generic;
using System.Text;
using FR8Runtime.Train;
using UnityEngine;

namespace FR8Runtime.Contracts.Predicates
{
    [AddComponentMenu("Contracts/Predicates/Delivery Predicate")]
    public class DeliveryPredicate : ContractPredicate
    {
        private int progress;

        public List<TrainCarriage> carriages;
        public TrackSection deliveryLocation;

        protected override string GetDisplay()
        {
            var sb = new StringBuilder();

            sb.Append("Deliver ");
            for (var i = 0; i < carriages.Count; i++)
            {
                if (i == 0) sb.Append($"{carriages[i].name}");
                else if (i == sb.Length - 1) sb.Append($", {carriages[i].name}");
                else sb.Append($", and {carriages[i].name}");
            }

            sb.Append($" to {deliveryLocation.name}");

            return sb.ToString();
        }

        protected override string ProgressString() => $"{progress}/{carriages.Count}";

        protected override int CalculateTasksDone()
        {
            progress = 0;
            foreach (var e in carriages)
            {
                if (!deliveryLocation.Contains(e.transform.position)) continue;
                progress++;
            }

            return progress;
        }

        protected override int CalculateTaskCount() => carriages.Count;
    }
}