using UnityEngine;

namespace FR8.Runtime.Gamemodes.Predicates
{
    [SelectionBase, DisallowMultipleComponent]
    public class DebugPredicate : ContractPredicate
    {
        public int progress = 0;
        public int maxProgress = 1;

        protected override int CalculateTasksDone() => Mathf.Clamp(progress, 0, maxProgress);

        protected override int CalculateTaskCount() => Mathf.Max(1, maxProgress);

        protected override string GetDisplay() => name;
    }
}