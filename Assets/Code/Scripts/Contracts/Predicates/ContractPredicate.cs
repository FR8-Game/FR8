using System;

namespace FR8Runtime.Contracts.Predicates
{
    [Serializable]
    public abstract class ContractPredicate
    {
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
    }
}