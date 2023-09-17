using System;
using System.Collections.Generic;
using System.Text;
using FR8Runtime.Train;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts.Predicates
{
    [CreateAssetMenu(menuName = ScriptableObjectLocation + "Delivery Predicate")]
    public class DeliveryPredicate : ContractPredicate
    {
        private int carriagesDelivered;

        public string[] carriageNames;
        public string deliveryLocationName;

        protected override string BuildString()
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

        protected override string ProgressString() => $"{carriagesDelivered}/{carriageNames.Length}";

        public override VisualElement BuildUI()
        {
            var root = new ProgressBar();

            root.title = ToString();
            root.lowValue = 0.0f;
            root.highValue = carriageNames.Length;
            root.value = carriagesDelivered;

            return root;
        }

        protected override int TasksDone()
        {
            var carriages = new List<TrainCarriage>();
            var deliveryLocation = GameObject.Find(deliveryLocationName).GetComponent<TrackSection>();

            foreach (var name in carriageNames)
            {
                var find = GameObject.Find(name);
                if (find) carriages.Add(find.GetComponent<TrainCarriage>());
            }

            carriagesDelivered = 0;
            foreach (var e in carriages)
            {
                if (deliveryLocation.Contains(e.transform.position)) carriagesDelivered++;
            }

            return carriagesDelivered;
        }

        protected override int TaskCount() => carriageNames.Length;
    }
}