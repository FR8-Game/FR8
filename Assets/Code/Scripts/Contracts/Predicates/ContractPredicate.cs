using System;
using System.Text;
using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts.Predicates
{
    public abstract class ContractPredicate : MonoBehaviour
    {
        [SerializeField] private DisplayMode displayMode = DisplayMode.Fraction;
        
        public const string ScriptableObjectLocation = "Scriptable Objects/Contracts/";

        public int TasksDone { get; private set; }
        public int TaskCount { get; private set; }

        public float Progress { get; private set; }
        public bool Done { get; private set; }
        public bool Dirty { get; private set; }

        public void Update()
        {
            var tasksDone = CalculateTasksDone();
            var taskCount = CalculateTaskCount();

            Progress = (float)tasksDone / taskCount;
            Done = tasksDone == taskCount;

            if (tasksDone == TasksDone && taskCount == TaskCount) return;

            MarkDirty();
            
            TasksDone = tasksDone;
            TaskCount = taskCount;
        }

        protected abstract int CalculateTasksDone();
        protected abstract int CalculateTaskCount();

        protected abstract string BuildString();
        protected virtual string ProgressString() => displayMode switch
        {
            DisplayMode.Percent => StringUtility.Percent(Progress),
            DisplayMode.Fraction => $"{TasksDone}/{TaskCount}",
            _ => throw new ArgumentOutOfRangeException()
        };

        public sealed override string ToString() => ToString(false);
        public string ToString(bool withTags)
        {
            return $"{BuildString()} {tag("<nobr>")}[{(Done ? "Done" : ProgressString())}]{tag("</nobr>")}".ToUpper();
            
            string tag(string tag) => withTags ? tag : string.Empty;
        }

        public void BuildUI(StringBuilder sb)
        {
            if (Done) sb.Append("<color=#00ff00>");
            sb.Append($"> {ToString(true)}");
            sb.AppendLine(Done ? "</color>" : string.Empty);
        }

        public void MarkDirty() => Dirty = true;
        public void ClearDirty() => Dirty = false;
        
        public enum DisplayMode
        {
            Fraction = default,
            Percent = 1,
        }
    }
}