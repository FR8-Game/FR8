using System;
using System.Xml.Serialization;

namespace FR8Runtime.Contracts.Predicates
{
    [Serializable]
    public abstract class ContractPredicate
    {
        [XmlIgnore]
        public float Progress { get; private set; }
        
        [XmlIgnore]
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