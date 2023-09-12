using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts.Predicates
{
    public abstract class ContractPredicate : ScriptableObject
    {
        public const string ScriptableObjectLocation = "Scriptable Objects/Contracts/";
        
        public float Progress { get; private set; }
        public bool Done { get; private set; }

        public void Update()
        {
            var tasksDone = TasksDone();
            var taskCount = TaskCount();
            
             Progress = (float)tasksDone / taskCount;
            Done = tasksDone == taskCount;
        }
        
        protected abstract int TasksDone();
        protected abstract int TaskCount();

        protected abstract string BuildString();
        protected virtual string ProgressString() => StringUtility.Percent(Progress);

        public sealed override string ToString()
        {
            return $"{BuildString()} [{(Done ? "Done" : ProgressString().PadLeft(4))}]".ToUpper();
        }

        public abstract VisualElement BuildUI();
    }
}