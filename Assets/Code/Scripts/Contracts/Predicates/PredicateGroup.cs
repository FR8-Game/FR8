using System.Collections.Generic;
using UnityEngine;

namespace FR8Runtime.Contracts.Predicates
{
    [SelectionBase, DisallowMultipleComponent]
    public class PredicateGroup : ContractPredicate
    {
        private List<ContractPredicate> children;

        private void Awake()
        {
            children = new List<ContractPredicate>();
            foreach (Transform child in transform)
            {
                if (!child.TryGetComponent(out ContractPredicate predicate)) continue;
                children.Add(predicate);
            }
        }

        protected override int CalculateTasksDone()
        {
            var count = 0;
            foreach (var child in children)
            {
                if (child.Done) count++;
            }

            return count;
        }

        protected override int CalculateTaskCount() => children.Count;

        protected override string BuildString()
        {
            var str = string.Empty;
            for (var i = 0; i < children.Count; i++)
            {
                str += children.Count;
                if (i != children.Count - 1) str += '\n';
            }
            return str;
        }
    }
}