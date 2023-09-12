using FR8Runtime.CodeUtility;
using UnityEngine;

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

        public sealed override string ToString()
        {
            return $"{BuildString()} [{(Done ? "Done" : StringUtility.Percent(Progress).PadLeft(4))}%]";
        }
    }
}