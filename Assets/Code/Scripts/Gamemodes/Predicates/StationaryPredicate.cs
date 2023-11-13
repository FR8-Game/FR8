using System.Collections.Generic;
using System.Text;
using FR8.Runtime.Train;

namespace FR8.Runtime.Gamemodes.Predicates
{
    public class StationaryPredicate : ContractPredicate
    {
        private int progress;

        public List<TrainCarriage> carriages;
        public List<CarriageConnector> highlighted;

        protected override string GetDisplay()
        {
            var sb = new StringBuilder();

            sb.Append("Apply handbrake and Decouple ");
            for (var i = 0; i < carriages.Count; i++)
            {
                if (i == 0) sb.Append($"{carriages[i].name}");
                else if (i == sb.Length - 1) sb.Append($", {carriages[i].name}");
                else sb.Append($", and {carriages[i].name}");
            }

            return sb.ToString();
        }

        protected override string ProgressString() => $"{progress}/{carriages.Count}";

        protected override int CalculateTasksDone()
        {
            progress = 0;
            var locomotives = new List<Locomotive>();
            foreach (var e in carriages)
            {
                if (!e.IsStationary(locomotives)) continue;
                progress++;
            }

            foreach (var locomotive in locomotives)
            {
                foreach (var connector in locomotive.CarriageConnectors)
                {
                    if (!connector.Connection) continue;
                    
                    connector.Highlight(true);
                    connector.Connection.Highlight(true);
                    
                    if (!highlighted.Contains(connector)) highlighted.Add(connector);
                    if (!highlighted.Contains(connector.Connection)) highlighted.Add(connector.Connection);
                }
            }

            return progress;
        }

        public override void OnTaskDone()
        {
            foreach (var e in highlighted)
            {
                e.Highlight(false);
            }
        }

        protected override int CalculateTaskCount() => carriages.Count;
    }
}