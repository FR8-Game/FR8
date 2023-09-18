using System;
using System.Collections.Generic;
using System.Text;
using FR8Runtime.Train;
using UnityEditor;
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

        private bool initialized;
        private List<TrainCarriage> carriages;
        private TrackSection deliveryLocation;
        private ProgressBar uiProgressBar;

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
            uiProgressBar = new ProgressBar();
            uiProgressBar.title = ToString();
            return uiProgressBar;
        }

        public override void UpdateUI()
        {
            uiProgressBar.lowValue = 0.0f;
            uiProgressBar.highValue = carriageNames.Length;
            uiProgressBar.value = carriagesDelivered;
        }

        protected override int TasksDone()
        {
            if (!initialized) Initialize();
            
            carriagesDelivered = 0;
            foreach (var e in carriages)
            {
                if (deliveryLocation.Contains(e.transform.position)) carriagesDelivered++;
            }

            return carriagesDelivered;
        }

        private void Initialize()
        {
            carriages = new List<TrainCarriage>();
            deliveryLocation = GameObject.Find(deliveryLocationName).GetComponent<TrackSection>();

            foreach (var name in carriageNames)
            {
                var find = GameObject.Find(name);
                if (find) carriages.Add(find.GetComponent<TrainCarriage>());
            }
        }

        protected override int TaskCount() => carriageNames.Length;
    }
}