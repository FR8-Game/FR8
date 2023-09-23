using System.Collections.Generic;
using System.Text;
using FR8Runtime.Train;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts.Predicates
{
    [AddComponentMenu("Contracts/Predicates/Delivery Predicate")]
    public class DeliveryPredicate : ContractPredicate
    {   
        private int carriagesDelivered;

        public List<TrainCarriage> carriages;
        public TrackSection deliveryLocation;

        private ProgressBar uiProgressBar;

        protected override string BuildString()
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

        protected override string ProgressString() => $"{carriagesDelivered}/{carriages.Count}";

        public override VisualElement BuildUI()
        {
            uiProgressBar = new ProgressBar();
            uiProgressBar.title = ToString(true);
            return uiProgressBar;
        }

        public override void UpdateUI()
        {
            uiProgressBar.lowValue = 0.0f;
            uiProgressBar.highValue = carriages.Count;
            uiProgressBar.value = carriagesDelivered;
        }

        protected override int TasksDone()
        {
            carriagesDelivered = 0;
            foreach (var e in carriages)
            {
                if (!e.Stationary) continue;
                if (!deliveryLocation.Contains(e.transform.position)) continue;

                carriagesDelivered++;
            }

            return carriagesDelivered;
        }

        protected override int TaskCount() => carriages.Count;
    }
}