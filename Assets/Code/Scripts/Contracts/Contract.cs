using FR8Runtime.Contracts.Predicates;
using UnityEngine;

namespace FR8Runtime.Contracts
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Contract")]
    public abstract class Contract : ScriptableObject
    {
        public ContractPredicate[] predicates;
        
        public float Progress { get; private set; }
        public int PredicatesCompleted { get; private set; }
        public bool Done => PredicatesCompleted == predicates.Length;
        
        public void Update()
        {
            PredicatesCompleted = 0;
            foreach (var e in predicates)
            {
                e.Update();
                Progress += e.Progress / predicates.Length;
                PredicatesCompleted++;
            }
        }

        public string BuildTitle() => $"{name} [{PredicatesCompleted}/{predicates.Length}]";
        
        public override string ToString()
        {
            var str = $"{BuildTitle()}\n";
            foreach (var e in predicates)
            {
                str += e.ToString();
            }
            
            return str;
        }
    }
}