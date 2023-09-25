using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts.Predicates
{
    public abstract class ContractPredicate : MonoBehaviour
    {
        public const string ScriptableObjectLocation = "Scriptable Objects/Contracts/";

        private int tasksDone;
        private int taskCount;

        public float Progress { get; private set; }
        public bool Done { get; private set; }

        public void Update()
        {
            var tasksDone = TasksDone();
            var taskCount = TaskCount();

            Progress = (float)tasksDone / taskCount;
            Done = tasksDone == taskCount;

            if (tasksDone == this.tasksDone && taskCount == this.taskCount) return;
            
            UpdateUI();
            this.tasksDone = tasksDone;
            this.taskCount = taskCount;
        }

        public abstract void UpdateUI();

        protected abstract int TasksDone();
        protected abstract int TaskCount();

        protected abstract string BuildString();
        protected virtual string ProgressString() => StringUtility.Percent(Progress);

        public sealed override string ToString() => ToString(false);
        public string ToString(bool withTags)
        {
            return $"{BuildString()} {tag("<nobr>")}[{(Done ? "Done" : ProgressString())}]{tag("</nobr>")}".ToUpper();
            
            string tag(string tag) => withTags ? tag : string.Empty;
        }

        public abstract VisualElement BuildUI();
    }
}