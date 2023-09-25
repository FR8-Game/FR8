using System.Collections.Generic;
using System.Text;
using FR8Runtime.Contracts.Predicates;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts
{
    [AddComponentMenu("Contracts/Contract")]
    public class Contract : MonoBehaviour
    {
        private const int divLength = 20;

        private string displayName;
        private List<ContractPredicate> predicates;

        public List<ContractPredicate> Predicates
        {
            get
            {
                if (!Application.isPlaying || predicates == null)
                {
                    predicates = new List<ContractPredicate>();
                    foreach (Transform child in transform)
                    {
                        if (!child.TryGetComponent(out ContractPredicate predicate)) continue;
                        predicates.Add(predicate);
                    }
                }

                return predicates;
            }
        }

        public float Progress { get; private set; }
        public int PredicatesCompleted { get; private set; }
        public bool Done => PredicatesCompleted == Predicates.Count;

        public static readonly List<Contract> ActiveContracts = new();

        private void OnEnable()
        {
            ActiveContracts.Add(this);
        }

        private void OnDisable()
        {
            ActiveContracts.Remove(this);
        }

        public void Update()
        {
            PredicatesCompleted = 0;
            foreach (var e in Predicates)
            {
                Progress += e.Progress / Predicates.Count;
                if (!e.Done) continue;
                PredicatesCompleted++;
            }
        }

        public string BuildTitle()
        {
            var name = !string.IsNullOrWhiteSpace(displayName) ? displayName : "Contract";
            return $"{name} [{(PredicatesCompleted == predicates.Count ? "Done" : $"{PredicatesCompleted}/{Predicates.Count}")}]".ToUpper();
        }

        public override string ToString()
        {
            var str = $"{BuildTitle()}\n";
            foreach (var e in Predicates)
            {
                str += e.ToString();
            }

            return str;
        }

        public string BuildUI()
        {
            var sb = new StringBuilder();

            if (Done) sb.Append("<color=#00ff00>");
            
            div();
            sb.AppendLine(BuildTitle());

            foreach (var e in predicates)
            {
                e.BuildUI(sb);
            }
            div();

            if (Done) sb.Append("</color>");
            return sb.ToString();

            void div() => sb.AppendLine(new string('-', divLength));
        }
    }
}