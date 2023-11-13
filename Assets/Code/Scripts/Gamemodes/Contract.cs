using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FR8.Runtime.Editor;
using FR8.Runtime.Gamemodes.Predicates;
using FR8.Runtime.Save;
using UnityEngine;

namespace FR8.Runtime.Gamemodes
{
    [AddComponentMenu("Contracts/Contract")]
    public class Contract : MonoBehaviour, IEnumerable<ContractPredicate>
    {
        private const int divLength = 20;

        public int doubloons = 5000;
        
        private string displayName;
        private List<ContractPredicate> predicateTree;
        private int activePredicateIndex;
        
        public float StartTime { get; private set; }

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

        public static event Action<Contract> ContractCompleteEvent;

        private void OnEnable()
        {
            ActiveContracts.Add(this);
            StartTime = Time.time;
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

            for (var i = 0; i < predicateTree.Count; i++)
            {
                var predicate = PredicateTree[i];
                predicate.IsActive = i <= activePredicateIndex;
            }

            if (PredicatesCompleted == predicateTree.Count)
            {
                CompleteContract();
            }
        }

        private void CompleteContract()
        {
            var save = SaveManager.ProgressionSave.GetOrLoad();
            save.doubloons += doubloons;
            SaveManager.ProgressionSave.Save();

            gameObject.SetActive(false);
            ContractCompleteEvent?.Invoke(this);
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
                predicate.gameObject.SetActive(false);
            }
        }

        public IEnumerator<ContractPredicate> GetEnumerator() => predicateTree.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}