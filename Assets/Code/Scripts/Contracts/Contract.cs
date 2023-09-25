using System.Collections.Generic;
using FR8Runtime.Contracts.Predicates;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts
{
    [AddComponentMenu("Contracts/Contract")]
    public class Contract : MonoBehaviour
    {
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
                PredicatesCompleted++;
            }
        }

        public string BuildTitle() => $"{name} [{PredicatesCompleted}/{Predicates.Count}]".ToUpper();
        
        public override string ToString()
        {
            var str = $"{BuildTitle()}\n";
            foreach (var e in Predicates)
            {
                str += e.ToString();
            }
            
            return str;
        }

        public VisualElement BuildUI()
        {
            var root = new VisualElement();
            root.AddToClassList("contract");

            root.style.marginLeft = 10;
            root.style.marginBottom = 6;

            var header = new Label(BuildTitle());
            root.name = "#header";
            root.Add(header);

            var content = new VisualElement();
            content.name = "#predicates";
            content.style.marginLeft = 10;
            content.style.maxWidth = 350;
            root.Add(content);

            foreach (var e in Predicates)
            {
                content.Add(e.BuildUI());
                e.UpdateUI();
            }

            return root;
        }
    }
}