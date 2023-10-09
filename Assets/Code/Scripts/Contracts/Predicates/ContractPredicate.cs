using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FR8Runtime.Contracts.Predicates
{
    public abstract class ContractPredicate : MonoBehaviour
    {
        [SerializeField] [TextArea] private string overrideDisplayText;
        [SerializeField] private DisplayMode displayMode = DisplayMode.Fraction;
        [SerializeField] private bool persistant;

        public const string ScriptableObjectLocation = "Scriptable Objects/Contracts/";
        
        public int TasksDone { get; private set; }
        public int TaskCount { get; private set; }

        public float Progress { get; private set; }
        public bool Done { get; private set; }

        public float CompletedTime { get; private set; }

        public bool Dirty { get; private set; }

        public void Update()
        {
            if (Done && !persistant) return;
            
            var tasksDone = CalculateTasksDone();
            var taskCount = CalculateTaskCount();

            Progress = (float)tasksDone / taskCount;
            var isDone = tasksDone == taskCount;

            if (!isDone) CompletedTime = 0.0f;
            else if (!Done) CompletedTime = Time.time;

            Done = isDone;

            if (tasksDone == TasksDone && taskCount == TaskCount) return;

            MarkDirty();

            TasksDone = tasksDone;
            TaskCount = taskCount;
        }

        protected abstract int CalculateTasksDone();
        protected abstract int CalculateTaskCount();

        protected virtual string GetDisplay() => $"Type {GetType().Name} Does not support automatic naming";

        protected virtual string ProgressString() => displayMode switch
        {
            DisplayMode.Percent => StringUtility.Percent(Progress),
            DisplayMode.Fraction => $"{TasksDone}/{TaskCount}",
            DisplayMode.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException()
        };

        public sealed override string ToString() => ToString(false);
        
        public string ToString(bool withTags)
        {
            var str = string.IsNullOrWhiteSpace(overrideDisplayText) ? GetDisplay() : overrideDisplayText;
            
            return $"{str} {tag("<nobr>")}[{(Done ? "Done" : ProgressString())}]{tag("</nobr>")}".ToUpper();

            string tag(string tag) => withTags ? tag : string.Empty;
        }

        public void BuildUI(StringBuilder sb, int index, bool current)
        {
            if (Done) sb.Append("<color=#00ff00>");
            sb.Append($"{(current ? '>' : ' ')}{index + 1}. {ToString(true)}");
            sb.AppendLine(Done ? "</color>" : string.Empty);
        }

        public void MarkDirty() => Dirty = true;
        public void ClearDirty() => Dirty = false;

        public enum DisplayMode
        {
            Fraction = default,
            Percent = 1,
            None = 2,
        }
    }
}