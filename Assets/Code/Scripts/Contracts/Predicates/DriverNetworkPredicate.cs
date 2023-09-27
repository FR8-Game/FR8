using System;
using System.Data.Common;
using FR8Runtime.Interactions.Drivers;
using FR8Runtime.Train;
using UnityEngine;

namespace FR8Runtime.Contracts.Predicates
{
    [SelectionBase, DisallowMultipleComponent]
    public class DriverNetworkPredicate : ContractPredicate
    {
        [Header("TARGET")]
        [SerializeField] private DriverNetwork driverNetwork;
        [SerializeField] private string key;
        
        [Header("SETTINGS")]
        [Space]
        [SerializeField] private Comparison comparison;
        [SerializeField] private float targetValue;
        [SerializeField] private float bias = 0.1f;
        [SerializeField] private TargetValueDisplayMode targetValueDisplayMode;
        [SerializeField] private float fallback;

        [Header("EVALUATION RESULTS")]
        [Space]
        [SerializeField] private int a;
        [SerializeField] private int b;

        private void Reset()
        {
            driverNetwork = FindObjectOfType<Locomotive>().GetComponent<DriverNetwork>();
        }

        protected override string GetDisplay()
        {
            if (!driverNetwork) return $"Contract \"{name}\" is missing a target";

            var value = GetValue();
            var delta = targetValue - value;

            string str;
            if (targetValueDisplayMode == TargetValueDisplayMode.Switch)
            {
                return $"Switch {key} {formatValue(targetValue)}";
            }
            
            str = delta > 0.0f ? "Increase" : "Decrease";
            str += $" Locomotive's {key} to ";

            str += comparison switch
            {
                Comparison.Equal => "",
                Comparison.Less => "Less Than",
                Comparison.Greater => "At Least",
                _ => throw new ArgumentOutOfRangeException()
            };

            str += $" {formatValue(targetValue)}";
            
            return str;

            string formatValue(float v) => targetValueDisplayMode switch
            {
                TargetValueDisplayMode.Default => (Mathf.Round(v / bias) * bias).ToString(),
                TargetValueDisplayMode.Percent => $"{v * 100.0f:N0}%",
                TargetValueDisplayMode.Switch => v > 0.5f ? "On" : "Off",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override int CalculateTasksDone() => Compare(GetValue()) ? 1 : 0;
        protected override int CalculateTaskCount() => 1;
        private float GetValue() => driverNetwork ? driverNetwork.GetValue(key, fallback) : fallback;

        public bool Compare(float value)
        {
            a = Mathf.RoundToInt(value / bias);
            b = Mathf.RoundToInt(targetValue / bias);
            
            return comparison switch
            {
                Comparison.Equal => a == b,
                Comparison.Less => a <= b,
                Comparison.Greater => a >= b,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public enum Comparison
        {
            Equal,
            Less,
            Greater,
        }

        public enum TargetValueDisplayMode
        {
            Default,
            Percent,
            Switch,
        }
    }
}