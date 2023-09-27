using System.Collections;
using System.Collections.Generic;
using System.Text;
using FR8Runtime.Contracts.Predicates;
using UnityEngine;

namespace FR8Runtime.Contracts
{
    [AddComponentMenu("Contracts/Contract")]
    public class Contract : MonoBehaviour, IEnumerable<ContractPredicate>
    {
        private const int divLength = 20;

        private string displayName;
        private List<ContractPredicate> predicateTree;
        private int activePredicateIndex;

        public List<ContractPredicate> PredicateTree
        {
            get
            {
                if (!Application.isPlaying || predicateTree == null)
                {
                    FindPredicates();
                }

                return predicateTree;
            }
        }
        public ContractPredicate ActivePredicate => activePredicateIndex >= 0 && activePredicateIndex < PredicateTree.Count ? PredicateTree[activePredicateIndex] : null;

        public float Progress { get; private set; }
        public int PredicatesCompleted { get; private set; }
        public bool Done => PredicatesCompleted == PredicateTree.Count;

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
            activePredicateIndex = 0;
            PredicatesCompleted = 0;
            Progress = 0.0f;
            
            for (var i = 0; i < PredicateTree.Count; i++)
            {
                var predicate = PredicateTree[i];
                Progress += predicate.Progress / PredicateTree.Count;
                if (!predicate.Done) break;

                activePredicateIndex++;
                PredicatesCompleted++;
            }
        }

        public string BuildTitle()
        {
            var name = !string.IsNullOrWhiteSpace(displayName) ? displayName : "Active Contract";
            return $"{name} [{(PredicatesCompleted == predicateTree.Count ? "Done" : $"{PredicatesCompleted}/{PredicateTree.Count}")}]".ToUpper();
        }

        public override string ToString()
        {
            var str = $"{BuildTitle()}\n";
            foreach (var e in PredicateTree)
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

            if (!Done) buildPredicate(activePredicateIndex - 1);
            buildPredicate(activePredicateIndex);
            
            div();

            if (Done) sb.Append("</color>");
            return sb.ToString();

            void div() => sb.AppendLine(new string('-', divLength));
            void buildPredicate(int i)
            {
                if (i < 0) return;
                if (i >= predicateTree.Count) return;
                
                predicateTree[i].BuildUI(sb, i, i == activePredicateIndex);
            }
        }
        
        private void FindPredicates()
        {
            predicateTree = new List<ContractPredicate>();
            foreach (Transform child in transform)
            {
                if (!child.TryGetComponent(out ContractPredicate predicate)) continue;
                predicateTree.Add(predicate);
            }
        }

        public IEnumerator<ContractPredicate> GetEnumerator() => predicateTree.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}