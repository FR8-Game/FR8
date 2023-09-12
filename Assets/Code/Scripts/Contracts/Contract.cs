using System.Collections.Generic;
using FR8Runtime.Contracts.Predicates;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Contract")]
    public class Contract : ScriptableObject
    {
        public List<ContractPredicate> predicates;
        
        public float Progress { get; private set; }
        public int PredicatesCompleted { get; private set; }
        public bool Done => PredicatesCompleted == predicates.Count;
        
        public void Update()
        {
            PredicatesCompleted = 0;
            foreach (var e in predicates)
            {
                e.Update();
                Progress += e.Progress / predicates.Count;
                PredicatesCompleted++;
            }
        }

        public string BuildTitle() => $"{name} [{PredicatesCompleted}/{predicates.Count}]".ToUpper();
        
        public override string ToString()
        {
            var str = $"{BuildTitle()}\n";
            foreach (var e in predicates)
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

            foreach (var e in predicates)
            {
                content.Add(e.BuildUI());
            }

            return root;
        }
    }
}