using System.Runtime.Serialization;
using FR8Runtime.Contracts;
using FR8Runtime.Player;
using UnityEditor;
using UnityEngine.UIElements;

namespace FR8Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerContractManager))]
    public class PlayerContractManagerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new IMGUIContainer(() => base.OnInspectorGUI()));

            if (targets.Length > 1) return root;

            var player = target as PlayerContractManager;
            if (!player) return root;

            if (player.ActiveContracts != null)
            {
                foreach (var contract in player.ActiveContracts)
                {
                    AddContract(contract, root);
                }
            }

            return root;
        }

        private void AddContract(Contract contract, VisualElement root)
        {
            if (!contract) return;

            var container = new VisualElement();
            root.Add(container);

            var header = new ProgressBar();
            header.title = contract.ToString();
            header.lowValue = 0.0f;
            header.highValue = 1.0f;
            header.value = contract.Progress;

            header.style.fontSize = 24;
            root.Add(header);

            foreach (var e in contract.predicates)
            {
                var bar = new ProgressBar();
                bar.style.marginLeft = 10;
                bar.title = e.ToString().ToUpper();
                bar.value = e.Progress;
                bar.lowValue = 0.0f;
                bar.highValue = 1.0f;

                container.Add(bar);
            }
        }
    }
}